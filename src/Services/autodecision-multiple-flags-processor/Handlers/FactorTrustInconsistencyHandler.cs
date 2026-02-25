using AutodecisionCore.Contracts.Enums;
using AutodecisionCore.Contracts.Messages;
using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionMultipleFlagsProcessor.Extensions;
using AutodecisionMultipleFlagsProcessor.Services;
using MassTransit;

namespace AutodecisionMultipleFlagsProcessor.Handlers
{
    public class FactorTrustInconsistencyHandler: IConsumer<ProcessFlagRequestEvent>
    {
        private readonly ILogger<FactorTrustInconsistencyHandler> _logger;
        private readonly IFlagHelper _flagHelper;

        public FactorTrustInconsistencyHandler(ILogger<FactorTrustInconsistencyHandler> logger, IFlagHelper flagHelper)
        {
            _logger = logger;
            _flagHelper = flagHelper;
        }

        public async Task Consume(ConsumeContext<ProcessFlagRequestEvent> context)
        {
            var autodecisionComposite = await _flagHelper.GetAutodecisionCompositeData(context.Message.Key);

            if (!_flagHelper.IsValidVersion(context.Message, autodecisionComposite, FlagCode.FactorTrustInconsistency, _logger))
                return;

            var response = ProcessFlag(autodecisionComposite);
            await _flagHelper.SendReponseMessage(response);
        }

        public ProcessFlagResponseEvent ProcessFlag(AutodecisionCompositeData autodecisionCompositeData)
        {
            var response = _flagHelper.BuildFlagResponse(FlagCode.FactorTrustInconsistency, autodecisionCompositeData);

            try
            {
                if (autodecisionCompositeData.FactorTrust is null || !autodecisionCompositeData.FactorTrust.HasFactorTrustInconsistency.HasValue) 
                {
                    response.FlagResult = FlagResultEnum.Ignored;
                    response.Message = "No active records found for this loan number.";
                    return response;
                }

                var military_active_duty = string.IsNullOrEmpty(autodecisionCompositeData.FactorTrust.Mla) ? string.Empty : autodecisionCompositeData.FactorTrust.Mla.Trim().ToLower();

                if (autodecisionCompositeData.FactorTrust.HasFactorTrustInconsistency.Value == true)
                {
                    response.FlagResult = FlagResultEnum.PendingApproval;
                    response.Message = "There is a possibility that the customer is a military officer on duty. Confirm by Factor Trust service.";
                    return response;
                }

                if (military_active_duty == "not active duty" || military_active_duty == "active duty")
                {
                    response.FlagResult = FlagResultEnum.Processed; 
                    return response;
                }

                response.FlagResult = FlagResultEnum.Ignored;
                response.Message = "Insufficient MLA data - Please verify it manually";
                return response;

            }
            catch (Exception e)
            {
                _logger.LogError(e, $"FlagCode: {FlagCode.FactorTrustInconsistency} was not successfully processed for LoanNumber: {autodecisionCompositeData.Application.LoanNumber} | Error: {e.Message}");

                response.Message = e.Message;
                response.FlagResult = FlagResultEnum.Error;
                return response;
            }
        }
    }
}
