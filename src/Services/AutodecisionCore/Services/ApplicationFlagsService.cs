using AutodecisionCore.Contracts.Enums;
using AutodecisionCore.Contracts.Messages;
using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionCore.Core.HandleFlagStrategies.Interfaces;
using AutodecisionCore.Data.Models.AutodecisionCoreAggregate;
using AutodecisionCore.Data.Repositories.Interfaces;
using AutodecisionCore.Extensions;
using AutodecisionCore.Services.Interfaces;
using System.Diagnostics;

namespace AutodecisionCore.Services
{
    public class ApplicationFlagsService : IApplicationFlagsService
    {
        private readonly IApplicationCoreRepository _applicationCoreRepository;
        private readonly ILogger<ApplicationFlagsService> _logger;
        private readonly IFlagRepository _flagRepository;
        private readonly ITriggerService _triggerService;
        private readonly IApplicationFlagsBinder _applicationFlagsBinder;

        public ApplicationFlagsService(
            IApplicationCoreRepository applicationCoreRepository,
            ILogger<ApplicationFlagsService> logger,
            IFlagRepository flagRepository,
            ITriggerService triggerService,
            IApplicationFlagsBinder applicationFlagsBinder
            )
        {
            _applicationCoreRepository = applicationCoreRepository;
            _logger = logger;
            _flagRepository = flagRepository;
            _triggerService = triggerService;
            _applicationFlagsBinder = applicationFlagsBinder;
        }

        public async Task AddApplicationFlagsToRegister(ApplicationCore applicationCore)
        {
            var flags = await _flagRepository.GetAllActiveFlagsExceptWarningAsync();

            foreach (var flag in flags)
            {
                applicationCore.AddFlag(flag.Code, flag.InternalFlag);
            }
        }

        public async Task AddApplicationFlagsToRegisterByIgnoringAsync(ApplicationCore applicationCore, string ignoredFlagCode, string description)
        {
            var flags = await _flagRepository.GetAllActiveFlagsExceptWarningAsync();

            foreach (var flag in flags)
            {
                if (flag.Code == ignoredFlagCode)
                {
                    applicationCore.AddFlag(flag.Code, flag.InternalFlag, FlagResultEnum.PendingApproval);
                    applicationCore.ApproveFlag(flag.Code, description, AutoDecisionUser.Id, AutoDecisionUser.Name);
                }
                else
                {
                    applicationCore.AddFlag(flag.Code, flag.InternalFlag);
                }
            }
        }

        public async Task AddApplicationFlagsToRegisterByIgnoringAsync(ApplicationCore applicationCore, string[] ignoredFlagCodes, string description)
        {
            var flags = await _flagRepository.GetAllActiveFlagsExceptWarningAsync();

            foreach (var flag in flags)
            {
                if (ignoredFlagCodes.Contains(flag.Code))
                {
                    applicationCore.AddFlag(flag.Code, flag.InternalFlag, FlagResultEnum.PendingApproval);
                    applicationCore.ApproveFlag(flag.Code, description, AutoDecisionUser.Id, AutoDecisionUser.Name);
                }
                else
                {
                    applicationCore.AddFlag(flag.Code, flag.InternalFlag);
                }
            }
        }

        public async Task AddOnlyInternalApplicationFlagsToRegister(ApplicationCore applicationCore)
        {
            var flags = await _flagRepository.GetAllActiveInternalFlagsExceptWarningAsync();

            foreach (var flag in flags)
            {
                applicationCore.AddFlag(flag.Code, flag.InternalFlag);
            }
        }

        public async Task FlagResponseStatusRegister(ProcessFlagResponseEvent response, ApplicationCore applicationCore)
        {
            try
            {
                _logger.LogInformation($"Loan Number: {response.LoanNumber} started processing for Flag: {response.FlagCode}");

                var timer = new Stopwatch();
                timer.Start();

                applicationCore.ReceiveFlagReponse(response.FlagCode, response.Message, response.ProcessedAt, (int)response.FlagResult, response.InternalMessages, response.ApprovalNote);
                await _applicationCoreRepository.SaveChanges();

                timer.Stop();
                TimeSpan timeTaken = timer.Elapsed;

                _logger.LogInformation($"Loan Number: {applicationCore.LoanNumber} finished processing for Flag: {response.FlagCode}. ElapsedTime: {timeTaken.ToString(@"m\:ss\.fff")} ");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error saving the Flag: {response.FlagCode} Result for Loan Number: {response.LoanNumber} | Error: {ex.Message}");
                throw;
            }
        }

        public async Task HandleApplicationFlags(Application application, ApplicationCore applicationCore, string reason)
        {
            await BindApplicationFlagsByTriggerAsync(applicationCore, reason);

            if (!applicationCore.ApplicationFlags.Any())
                await _applicationFlagsBinder.BindApplicationFlagsAsync(application, applicationCore);
        }

        public async Task BindApplicationFlagsByTriggerAsync(ApplicationCore applicationCore, string reason)
        {
            if (applicationCore.ApplicationFlags.Any())
            {
                var flags = await _triggerService.GetTriggerFlagCodesByProcessingReasonAsync(reason);
                applicationCore.ReprocessAllFlags(flags);
            }
        }
    }
}