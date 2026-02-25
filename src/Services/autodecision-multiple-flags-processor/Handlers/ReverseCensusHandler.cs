using AutodecisionCore.Contracts.Enums;
using AutodecisionCore.Contracts.Messages;
using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionMultipleFlagsProcessor.Extensions;
using AutodecisionMultipleFlagsProcessor.Services;
using MassTransit;

namespace AutodecisionMultipleFlagsProcessor.Handlers
{
    public class ReverseCensusHandler : IConsumer<ProcessFlagRequestEvent>
    {
        private readonly ILogger<ReverseCensusHandler> _logger;
        private readonly IFlagHelper _flagHelper;

        public ReverseCensusHandler(
            ILogger<ReverseCensusHandler> logger,
            IFlagHelper flagHelper
            )
        {
            _logger = logger;
            _flagHelper = flagHelper;
        }

        public async Task Consume(ConsumeContext<ProcessFlagRequestEvent> context)
        {
            var autodecisionComposite = await _flagHelper.GetAutodecisionCompositeData(context.Message.Key);

            if (!_flagHelper.IsValidVersion(context.Message, autodecisionComposite, FlagCode.ReverseCensus, _logger))
                return;

			var response = ProcessFlag(autodecisionComposite);
			response.Reason = context.Message.Reason;
            await _flagHelper.SendReponseMessage(response);
        }

        public ProcessFlagResponseEvent ProcessFlag(AutodecisionCompositeData autodecisionCompositeData)
        {
            var response = _flagHelper.BuildFlagResponse(FlagCode.ReverseCensus, autodecisionCompositeData, FlagResultEnum.Processed);
            try
            {
                if (!DefaultValidationsSucceeded(autodecisionCompositeData, response))
                    return response;
                //This flag remains raised continuously to signal this message to the UW
                if (autodecisionCompositeData.Census.CustomerId == 0)
                    _flagHelper.RaiseFlag(response, "Employee not found on reverse census");
                else
                    _flagHelper.RaiseFlag(response, "Employee found on census and eligible");

                return response;
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"FlagCode: {FlagCode.ReverseCensus} was not successfully processed for LoanNumber: {autodecisionCompositeData.Application.LoanNumber} | Error: {e.Message}");

                response.Message = e.Message;
                response.FlagResult = FlagResultEnum.Error;
                return response;
            }
        }

        private static bool DefaultValidationsSucceeded(AutodecisionCompositeData autodecisionCompositeData, ProcessFlagResponseEvent response)
        {
            if (!(autodecisionCompositeData.Census?.FlagReverseCensusActive).GetValueOrDefault(false))
            {
                response.FlagResult = FlagResultEnum.Ignored;
                return false;
            }
            return true;
        }
    }
}