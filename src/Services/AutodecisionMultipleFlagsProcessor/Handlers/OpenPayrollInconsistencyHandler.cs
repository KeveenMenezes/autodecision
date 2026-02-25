using AutodecisionCore.Contracts.Constants;
using AutodecisionCore.Contracts.Enums;
using AutodecisionCore.Contracts.Messages;
using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionMultipleFlagsProcessor.Extensions;
using AutodecisionMultipleFlagsProcessor.Helpers;
using AutodecisionMultipleFlagsProcessor.Services;
using BmgMoney.FeatureToggle.DotNetCoreClient.Interface;
using MassTransit;


namespace AutodecisionMultipleFlagsProcessor.Handlers
{
    public class OpenPayrollInconsistencyHandler : IConsumer<ProcessFlagRequestEvent>
    {
        private readonly ILogger<OpenPayrollInconsistencyHandler> _logger;
        private readonly IFlagHelper _flagHelper;
        private readonly IFeatureToggleClient _featureToggleClient;

        public OpenPayrollInconsistencyHandler(
            ILogger<OpenPayrollInconsistencyHandler> logger,
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
            if (!_flagHelper.IsValidVersion(context.Message, autodecisionComposite, FlagCode.OpenPayrollInconsistency, _logger)) return;
            var response = ProcessFlag(autodecisionComposite);
            await _flagHelper.SendReponseMessage(response);
        }

        public ProcessFlagResponseEvent ProcessFlag(AutodecisionCompositeData compositeData)
        {
            var response = _flagHelper.BuildFlagResponse(FlagCode.OpenPayrollInconsistency, compositeData);

            try
            {
                if (!_featureToggleClient.IsEnabled("Flags253And255"))
                {
                    response.FlagResult = FlagResultEnum.Ignored;
                    response.Message = "Flag not released.";
                    return response;
                }

                if (compositeData.Application.Type != ApplicationType.NewLoan)
                {
                    response.FlagResult = FlagResultEnum.Ignored;
                    response.Message = "Only new applications is allowed.";
                    return response;
                }

                response.FlagResult = FlagResultEnum.Processed;

                if (!FlagValidatorHelper.CanValidateFlag(compositeData, response))
                    return response;

                FlagValidatorHelper.HasOpenPayrollInconsistency(compositeData, response);

                return response;
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"FlagCode: {FlagCode.OpenPayrollInconsistency} was not successfully processed for LoanNumber: {compositeData.Application.LoanNumber} | Error: {e.Message}");

                response.Message = e.Message;
                response.FlagResult = FlagResultEnum.Error;
                return response;
            }
        }
    }
}