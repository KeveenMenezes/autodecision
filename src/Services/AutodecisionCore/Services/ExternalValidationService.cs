using AutodecisionCore.Contracts.Constants;
using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionCore.Data.HttpRepositories.Interfaces;
using AutodecisionCore.Extensions;
using AutodecisionCore.Services.Interfaces;
using BmgMoney.FeatureToggle.DotNetCoreClient.Interface;

namespace AutodecisionCore.Services
{
    public class ExternalValidationService : IExternalValidationService
    {
        private readonly ILogger<ExternalValidationService> _logger;
        private readonly IFeatureToggleClient _featureToggleClient;
        private readonly ICreditInquiryService _creditInquiryService;
        private readonly IClarityRepository _clarityRepository;
        private readonly ITransunionRepository _transunionRepository;

        public ExternalValidationService(
            ILogger<ExternalValidationService> logger,
            IFeatureToggleClient featureToggleClient,
            ICreditInquiryService creditInquiryService,
            IClarityRepository clarityRepository,
            ITransunionRepository transunionRepository)
        {
            _logger = logger;
            _featureToggleClient = featureToggleClient;
            _creditInquiryService = creditInquiryService;
            _clarityRepository = clarityRepository;
            _transunionRepository = transunionRepository;
        }

        public async Task Run(Application application)
        {
            try
            {
                if (application.ProductId == ApplicationProductId.Cashless)
                    return;

                if (_featureToggleClient.IsDisabled("stop_old_autodecision"))
                {
                    _logger.LogInformation($"Old autodecision is runing. Not going to external vendor.");
                    return;
                }

                _logger.LogInformation($"Starting Request to run External Validation Services: {application.LoanNumber}");


                if (application.Type == ApplicationType.NewLoan || _featureToggleClient.IsEnabled("system_enable_clarify_refinance"))
                {
                    await _creditInquiryService.RunClarityInquiry(application);

                    if (_featureToggleClient.IsEnabled("clarity_identity_priority"))
                    {
                        var clarityTransunionResult = await _transunionRepository.GetClarityTransunionResultInfo(application.CustomerId);
                        var clarity = await _clarityRepository.GetClarityInfo(application.LoanNumber);

                        if (clarity == null || clarityTransunionResult == null || clarity?.RequestHash != clarityTransunionResult?.RequestHash)
                        {
                            await _transunionRepository.DeactivateOldRecords(application.CustomerId);
                            await _transunionRepository.CreateNewTransunionResultFromClarity(application);
                        }

                        if (clarity == null || !_clarityRepository.IsClarityValid(clarity))
                            await _creditInquiryService.StartTransunionProcess(application);
                    }

                    if (application.Program == BMGMoneyProgram.LoansForFeds)
                        await _creditInquiryService.RunFedsDataCenter(application);

                    if (_featureToggleClient.IsDisabled("equifax_run_manually") && application.Program is BMGMoneyProgram.LoansForFeds or BMGMoneyProgram.LoansForAll)
                        await _creditInquiryService.RunEquifaxWorkNumberInquiry(application);
                }


                if (application.Type == ApplicationType.NewLoan || (application.Type == ApplicationType.Refi && _featureToggleClient.IsEnabled("ft_run_factor_trust_for_refi")))
                    await _creditInquiryService.RunFactorTrustInquiry(application);

                if (application.Program == BMGMoneyProgram.LoansAtWork || _featureToggleClient.IsDisabled("giact_run_manually"))
                    await _creditInquiryService.RunGiact(application);

                _logger.LogInformation($"Request to run External Validation Services Finished: {application.LoanNumber}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error when trying to execute the External Validations Service for loanNumber: {application.LoanNumber} | Error: {ex.Message}");
            }
        }

    }
}
