using AutodecisionCore.Contracts.Enums;
using AutodecisionCore.Contracts.Messages;
using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionMultipleFlagsProcessor.Extensions;
using AutodecisionMultipleFlagsProcessor.Services;
using MassTransit;

namespace AutodecisionMultipleFlagsProcessor.Handlers
{
    public class ActiveMilitaryDutyHandler : IConsumer<ProcessFlagRequestEvent>
    {
        private readonly ILogger<ActiveMilitaryDutyHandler> _logger;
        private readonly IFlagHelper _flagHelper;

        public ActiveMilitaryDutyHandler(ILogger<ActiveMilitaryDutyHandler> logger, IFlagHelper flagHelper)
        {
            _logger = logger;
            _flagHelper = flagHelper;
        }

        public async Task Consume(ConsumeContext<ProcessFlagRequestEvent> context)
        {
            var autodecisionComposite = await _flagHelper.GetAutodecisionCompositeData(context.Message.Key);

            if (!_flagHelper.IsValidVersion(context.Message, autodecisionComposite, FlagCode.ActiveMilitaryDuty, _logger))
                return;

            var response = ProcessFlag(autodecisionComposite);
			response.Reason = context.Message.Reason;
			await _flagHelper.SendReponseMessage(response);
        }

        public ProcessFlagResponseEvent ProcessFlag(AutodecisionCompositeData autodecisionCompositeData)
        {
            var response = _flagHelper.BuildFlagResponse(FlagCode.ActiveMilitaryDuty, autodecisionCompositeData);
            
            try
            {
                if (autodecisionCompositeData.FactorTrust is null)
                {
                    response.FlagResult = FlagResultEnum.Ignored;
                    return response;
                }

                var military_active_duty = string.IsNullOrEmpty(autodecisionCompositeData.FactorTrust.Mla) ? string.Empty : autodecisionCompositeData.FactorTrust.Mla.Trim().ToLower();

                if (military_active_duty == "active duty")
                {
                    if (autodecisionCompositeData.WhiteList is null)
                    {
                        response.FlagResult = FlagResultEnum.AutoDeny;
                        response.Message = "Customer is an active duty military and isn't on the White List";
                        return response;
                    }

                    if (string.IsNullOrEmpty(autodecisionCompositeData.WhiteList.Reason))
                    {
                        response.FlagResult = FlagResultEnum.AutoDeny;
                        response.Message = "Customer is an active duty military and doesn't have a White List reason";
                        return response;
                    }
                    response.FlagResult = FlagResultEnum.PendingApproval;
                    response.Message =  "Customer is an active duty military according to FactorTrust";
                    return response;
                }
                else if (military_active_duty != "not active duty")
                {
                    response.FlagResult = FlagResultEnum.Ignored;
                    response.Message = "Unable to check MLA - Please verify it manually";
                    return response;
                }

                response.FlagResult = FlagResultEnum.Processed;
                return response;

            }
            catch (Exception e)
            {
				_logger.LogError(e, $"FlagCode: {FlagCode.ActiveMilitaryDuty} was not successfully processed for LoanNumber: {autodecisionCompositeData.Application.LoanNumber} | Error: {e.Message}");

				response.Message = e.Message;
                response.FlagResult = FlagResultEnum.Error;
                return response;
            }
        }
    }
}