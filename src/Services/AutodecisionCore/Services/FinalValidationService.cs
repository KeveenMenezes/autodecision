using AutodecisionCore.Contracts.Messages;
using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionCore.Data.Models.AutodecisionCoreAggregate;
using AutodecisionCore.Data.Repositories.Interfaces;
using AutodecisionCore.Extensions;
using AutodecisionCore.Services.Interfaces;
using BmgMoney.FeatureToggle.DotNetCoreClient.Interface;
using System.Diagnostics;

namespace AutodecisionCore.Services
{
    public class FinalValidationService : IFinalValidationService
    {
        private readonly ILogger<FinalValidationService> _logger;
        private readonly IApplicationCoreRepository _applicationCoreRepository;
        private readonly IAutoApprovalService _autoApprovalService;
        private readonly IAutodecisionCompositeService _autodecisionCompositeService;
        private readonly IAutodecisionPublisherService _autodecisionPublisherService;
        private readonly IFeatureToggleClient _featureToggleClient;

        public FinalValidationService(
            ILogger<FinalValidationService> logger,
            IApplicationCoreRepository applicationCoreRepository,
            IAutoApprovalService autoApprovalService,
            IAutodecisionCompositeService autodecisionCompositeService,
            IAutodecisionPublisherService autodecisionPublisherService,
            IFeatureToggleClient featureToggleClient)
        {
            _logger = logger;
            _applicationCoreRepository = applicationCoreRepository;
            _autoApprovalService = autoApprovalService;
            _autodecisionCompositeService = autodecisionCompositeService;
            _autodecisionPublisherService = autodecisionPublisherService;
            _featureToggleClient = featureToggleClient;
        }

        public async Task Process(ProcessFinalValidation message, ApplicationCore applicationCore)
        {
            var timer = new Stopwatch();
            timer.Start();

            if (applicationCore.Status != InternalStatusEnum.Pending)
            {
                _logger.LogInformation($"Rejected. ApplicationCore status is {applicationCore.Status}. Loan Number: {message.LoanNumber}");
                return;
            }

            if (applicationCore.HasInProcessingFlags())
            {
                _logger.LogInformation($"Application has InProcessing flags. Codes: {string.Join(", ", applicationCore.InProcessingFlags())}. Loan Number: {message.LoanNumber}");
                return;
            }

            timer.Stop();
            TimeSpan timeTaken = timer.Elapsed;
            _logger.LogInformation($"Time to HasInProcessingFlags: {timeTaken.ToString(@"m\:ss\.fff")}. Loan Number: {message.LoanNumber}");
            timer.Restart();

            await SendToOpenConnections(applicationCore, message.Reason);

            timer.Stop();
            timeTaken = timer.Elapsed;
            _logger.LogInformation($"Time to SendToOpenConnections: {timeTaken.ToString(@"m\:ss\.fff")}. Loan Number: {message.LoanNumber}");
            timer.Restart();

            if (ApplicationProcessIsFinished(applicationCore, message.LoanNumber)) return;

            timer.Stop();
            timeTaken = timer.Elapsed;
            _logger.LogInformation($"Time to ApplicationProcessIsFinished: {timeTaken.ToString(@"m\:ss\.fff")}. Loan Number: {message.LoanNumber}");
            timer.Restart();

            if (await ValidateReason(message.LoanNumber, message.Reason)) return;

            timer.Stop();
            timeTaken = timer.Elapsed;
            _logger.LogInformation($"Time to ValidateReason: {timeTaken.ToString(@"m\:ss\.fff")}. Loan Number: {message.LoanNumber}");
            timer.Restart();

            if (await HasAutoDenyFlag(applicationCore, message.LoanNumber)) return;

            timer.Stop();
            timeTaken = timer.Elapsed;
            _logger.LogInformation($"Time to HasAutoDenyFlag: {timeTaken.ToString(@"m\:ss\.fff")}. Loan Number: {message.LoanNumber}");
            timer.Restart();

            var autodecisionCompositeData = await _autodecisionCompositeService.GetCompositeDataFromRedis(message.Key);

            timer.Stop();
            timeTaken = timer.Elapsed;
            _logger.LogInformation($"Time to GetCompositeDataFromRedis: {timeTaken.ToString(@"m\:ss\.fff")}. Loan Number: {message.LoanNumber}");
            timer.Restart();

            await _autoApprovalService.RunAutoApproval(applicationCore, autodecisionCompositeData, message.LoanNumber, message.Reason);

            timer.Stop();
            timeTaken = timer.Elapsed;
            _logger.LogInformation($"Time to RunAutoApproval: {timeTaken.ToString(@"m\:ss\.fff")}. Loan Number: {message.LoanNumber}");
            timer.Restart();

            if (await HasDenyAutoApprovalRules(applicationCore, message.LoanNumber)) return;

            timer.Stop();
            timeTaken = timer.Elapsed;
            _logger.LogInformation($"Time to HasDenyAutoApprovalRules: {timeTaken.ToString(@"m\:ss\.fff")}. Loan Number: {message.LoanNumber}");
            timer.Restart();

            if (_featureToggleClient.IsEnabled("FeatureLineAssignment") && _featureToggleClient.IsEnabled("Flags253And255"))
            {
                if (await NeedToRequestOldestPayStub(applicationCore, autodecisionCompositeData, message.LoanNumber)) return;

                timer.Stop();
                timeTaken = timer.Elapsed;
                _logger.LogInformation($"Time to NeedToRequestOldestPayStub: {timeTaken.ToString(@"m\:ss\.fff")}. Loan Number: {message.LoanNumber}");
                timer.Restart();
            }

            if (await HasPendingAprovalFlags(applicationCore, message.LoanNumber)) return;

            timer.Stop();
            timeTaken = timer.Elapsed;
            _logger.LogInformation($"Time to HasPendingAprovalFlags: {timeTaken.ToString(@"m\:ss\.fff")}. Loan Number: {message.LoanNumber}");
            timer.Restart();

            if (await NeedToRequestAllotment(applicationCore, autodecisionCompositeData, message.LoanNumber)) return;

            timer.Stop();
            timeTaken = timer.Elapsed;
            _logger.LogInformation($"Time to NeedToRequestAllotment: {timeTaken.ToString(@"m\:ss\.fff")}. Loan Number: {message.LoanNumber}");
            timer.Restart();

            await ApproveApplication(applicationCore, message.LoanNumber);

            timer.Stop();
            timeTaken = timer.Elapsed;
            _logger.LogInformation($"Time to ApproveApplication: {timeTaken.ToString(@"m\:ss\.fff")}. Loan Number: {message.LoanNumber}");
            timer.Restart();

            return;
        }

        #region private methods

        private bool ApplicationProcessIsFinished(ApplicationCore applicationCore, string loanNumber)
        {
            if (applicationCore.ApplicationProcessIsFinished())
            {
                _logger.LogWarning($"Final validation: The processing of this application has already been completed. LoanNumber: {loanNumber}");
                return true;
            }

            return false;
        }

        private async Task<bool> HasAutoDenyFlag(ApplicationCore applicationCore, string loanNumber)
        {
            if (applicationCore.HasAutoDenyFlag())
            {
                applicationCore.UpdateApplicationStatus(InternalStatusEnum.AutoDeny);
                await _applicationCoreRepository.SaveChanges();
                _logger.LogWarning($"Final validation: A Deny Flag was found. LoanNumber: {loanNumber}");
                await _autodecisionPublisherService.PublishDeclineApplicationRequestEvent(loanNumber, string.Empty);
                return true;
            }

            return false;
        }

        private async Task<bool> HasPendingAprovalFlags(ApplicationCore applicationCore, string loanNumber)
        {
            if (!applicationCore.CanAskForAllotmentOrProcessed())
            {
                applicationCore.UpdateApplicationStatus(InternalStatusEnum.PendingApproval);
                await _applicationCoreRepository.SaveChanges();
                _logger.LogWarning($"Final validation: Flags with pending approval were found. LoanNumber: {loanNumber}");
                await _autodecisionPublisherService.PublishRequireDocumentsRequestEvent(loanNumber, applicationCore.GetPendingApprovalFlagCodes());
                return true;
            }

            return false;
        }

        private async Task<bool> NeedToRequestAllotment(ApplicationCore applicationCore, AutodecisionCompositeData autodecisionCompositeData, string loanNumber)
        {
            if (applicationCore.HasAllotmentNeeded())
            {
                applicationCore.UpdateApplicationStatus(InternalStatusEnum.PendingDocuments);
                await _applicationCoreRepository.SaveChanges();
                _logger.LogWarning($"Final validation: The allotment is required. LoanNumber: {loanNumber}");
                await _autodecisionPublisherService.PublishRequireAllotmentRequestEvent(loanNumber, autodecisionCompositeData.Application.EmployerPaymentType);
                return true;
            }

            return false;
        }

        private async Task<bool> NeedToRequestOldestPayStub(ApplicationCore applicationCore, AutodecisionCompositeData autodecisionCompositeData, string loanNumber)
        {
            if (applicationCore.HasRequestOldPayStub() && !HasOldestPaystubDoc(autodecisionCompositeData))
            {
                var listFlag = new List<string>()
                {
                    FlagCode.OpenPayrollInconsistency
                };

                applicationCore.UpdateApplicationStatus(InternalStatusEnum.PendingApproval);
                await _applicationCoreRepository.SaveChanges();
                _logger.LogWarning($"Final validation: The paystub document is required. LoanNumber: {loanNumber}");
                await _autodecisionPublisherService.PublishRequireDocumentsRequestEvent(loanNumber, listFlag);
                return true;
            }

            return false;
        }

        private static bool HasOldestPaystubDoc(AutodecisionCompositeData autodecisionCompositeData) =>
            autodecisionCompositeData.FlagValidatorHelper.ApplicationDocuments.Any(d => d.DocumentType == DocumentType.OldestPaystub);

        private async Task<bool> HasDenyAutoApprovalRules(ApplicationCore applicationCore, string loanNumber)
        {
            if (applicationCore.HasDenyAutoApprovalRules())
            {
                applicationCore.UpdateApplicationStatus(InternalStatusEnum.AutoDeny);
                await _applicationCoreRepository.SaveChanges();
                _logger.LogWarning($"Final validation: AutoApproval failed to approve application. LoanNumber: {loanNumber}");
                await _autodecisionPublisherService.PublishDeclineApplicationRequestEvent(loanNumber, string.Empty);
                return true;
            }

            return false;
        }

        private async Task ApproveApplication(ApplicationCore applicationCore, string loanNumber)
        {
            applicationCore.UpdateApplicationStatus(InternalStatusEnum.AutoApproval);
            await _applicationCoreRepository.SaveChanges();
            _logger.LogInformation($"Final validation: Application was approved. LoanNumber: {loanNumber}");
            await _autodecisionPublisherService.PublishApproveApplicationRequestEvent(loanNumber);
        }

        private async Task<bool> ValidateReason(string loanNumber, string reason)
        {
            if (reason == Reason.DeclineInsuficientResponse)
            {
                await _autodecisionPublisherService.PublishDeclineApplicationRequestEvent(loanNumber, reason);
                return true;
            }

            return false;
        }

        private async Task SendToOpenConnections(ApplicationCore applicationCore, string reason)
        {
            _logger.LogInformation($"Final validation: Sending message to Open Connections. LoanNumber: {applicationCore.LoanNumber}");
            await _autodecisionPublisherService.PublishOpenConnectionsProcessRequest(applicationCore, reason);
        }

        #endregion
    }
}
