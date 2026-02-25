using AutodecisionCore.Contracts.Constants;
using AutodecisionCore.Contracts.Enums;
using AutodecisionCore.Contracts.Messages;
using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionMultipleFlagsProcessor.Extensions;
using AutodecisionMultipleFlagsProcessor.Services;
using MassTransit;

namespace AutodecisionMultipleFlagsProcessor.Handlers
{
    public class PhoneVoipHandler : IConsumer<ProcessFlagRequestEvent>
    {
        private readonly ILogger<PhoneVoipHandler> _logger;
        private readonly IFlagHelper _flagHelper;

        public PhoneVoipHandler(ILogger<PhoneVoipHandler> logger, IFlagHelper flagHelper)
        {
            _logger = logger;
            _flagHelper = flagHelper;
        }

        public async Task Consume(ConsumeContext<ProcessFlagRequestEvent> context)
        {
            var autodecisionComposite = await _flagHelper.GetAutodecisionCompositeData(context.Message.Key);

            if (!_flagHelper.IsValidVersion(context.Message, autodecisionComposite, FlagCode.PhoneVoip, _logger))
                return;

			var response = ProcessFlag(autodecisionComposite);
			response.Reason = context.Message.Reason;
            await _flagHelper.SendReponseMessage(response);
        }

        public ProcessFlagResponseEvent ProcessFlag(AutodecisionCompositeData autodecisionCompositeData)
        {
            var response = _flagHelper.BuildFlagResponse(FlagCode.PhoneVoip, autodecisionCompositeData);

            try
            {
                if (autodecisionCompositeData.Application.Type != ApplicationType.NewLoan)
                {
                    response.FlagResult = FlagResultEnum.Ignored;
                    return response;
                }

                if (autodecisionCompositeData.Customer.MobileNetworkType == MobileNetworkType.Voip)
                {
                    _flagHelper.RaiseFlag(response, "Mobile network type: Voip");

                    if (autodecisionCompositeData.WhiteList == null && autodecisionCompositeData.Application.TurndownActive)
                    {
                        response.FlagResult = FlagResultEnum.AutoDeny;
                        response.Message = "Application Denied";
                    }

                    return response;
                }

                response.FlagResult = FlagResultEnum.Processed;

                return response;
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"FlagCode: {FlagCode.PhoneVoip} was not successfully processed for LoanNumber: {autodecisionCompositeData.Application.LoanNumber} | Error: {e.Message}");

                response.Message = e.Message;
                response.FlagResult = FlagResultEnum.Error;
                return response;
            }
        }
    }
}
