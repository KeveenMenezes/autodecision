using AutodecisionCore.Contracts.Constants;
using AutodecisionCore.Contracts.Enums;
using AutodecisionCore.Contracts.Messages;
using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionMultipleFlagsProcessor.Extensions;
using AutodecisionMultipleFlagsProcessor.Helpers;
using AutodecisionMultipleFlagsProcessor.Services;
using AutodecisionMultipleFlagsProcessor.Utility;
using BmgMoney.FeatureToggle.DotNetCoreClient.Interface;
using MassTransit;

namespace AutodecisionMultipleFlagsProcessor.Handlers
{
    public class EmploymentLengthHandler : IConsumer<ProcessFlagRequestEvent>
    {
        private readonly ILogger<EmploymentLengthHandler> _logger;
        private readonly IFlagHelper _flagHelper;
        private readonly IFeatureToggleClient _featureToggleClient;
        public EmploymentLengthHandler(ILogger<EmploymentLengthHandler> logger,
            IFlagHelper flagHelper,
            IFeatureToggleClient featureToggleClient)
        {
            _logger = logger;
            _flagHelper = flagHelper;
            _featureToggleClient = featureToggleClient;
        }

        public async Task Consume(ConsumeContext<ProcessFlagRequestEvent> context)
        {
            var autodecisionComposite = await _flagHelper.GetAutodecisionCompositeData(context.Message.Key);

            if (!_flagHelper.IsValidVersion(context.Message, autodecisionComposite, FlagCode.EmploymentLength, _logger))
                return;
            var response = ProcessFlag(autodecisionComposite);
            response.Reason = context.Message.Reason;
            await _flagHelper.SendReponseMessage(response);
        }

        public ProcessFlagResponseEvent ProcessFlag(AutodecisionCompositeData compositeData)
        {
            var response = _flagHelper.BuildFlagResponse(FlagCode.EmploymentLength, compositeData, FlagResultEnum.Processed);

            try
            {
                int hireDateDays = GetHireDateDays(compositeData);
                int? minLengthOfEmployment = GetMinLengthOfEmployment(compositeData);

                if (minLengthOfEmployment is null)
                {
                    response.FlagResult = FlagResultEnum.Ignored;
                    response.Message = "The length of employment is not parameterized.";
                    return response;
                }

                if (hireDateDays < minLengthOfEmployment)
                {
                    string description = $"The minimum length of employment required is {minLengthOfEmployment} days, but the customer has only been employed for {hireDateDays} days.";
                    _flagHelper.RaiseFlag(response, description);

                    if (_featureToggleClient.IsEnabled("EnableAutoDenyFlag220"))
                    {
                        if (CanApplyAutoDeny(compositeData))
                        {
                            response.FlagResult = FlagResultEnum.AutoDeny;
                            response.Message = string.Concat("Application Denied. ", description);
                            return response;
                        }
                    }
                }
                return response;
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"FlagCode: {FlagCode.EmploymentLength} was not successfully processed for LoanNumber: {compositeData.Application.LoanNumber} | Error: {e.Message}");

                response.Message = e.Message;
                response.FlagResult = FlagResultEnum.Error;
                return response;
            }
        }

        private static int? GetMinLengthOfEmployment(AutodecisionCompositeData compositeData) =>
            compositeData.Application.Type == ApplicationType.NewLoan ? compositeData.CreditPolicy.CreditPolicyEntity.LoeNew : compositeData.CreditPolicy.CreditPolicyEntity.LoeRefi;

        private static int GetHireDateDays(AutodecisionCompositeData compositeData) =>
            compositeData.Application.VerifiedDateOfHire.GetDifferenceInDaysFromToday();

        private static bool CanApplyAutoDeny(AutodecisionCompositeData compositeData) =>
            FlagValidatorHelper.CanValidateFlag(compositeData)
            && !FlagValidatorHelper.HasOpenPayrollInconsistency(compositeData)
            && FlagValidatorHelper.CanAutoDenyCustomer(compositeData)
            && compositeData.Application.Type == ApplicationType.NewLoan;
    }
}