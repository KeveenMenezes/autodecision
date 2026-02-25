using AutodecisionCore.Contracts.Enums;
using AutodecisionCore.Contracts.Messages;
using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionMultipleFlagsProcessor.Extensions;
using AutodecisionMultipleFlagsProcessor.Services;
using MassTransit;

namespace AutodecisionMultipleFlagsProcessor.Handlers
{
    public class ProbableCashAppHandler : IConsumer<ProcessFlagRequestEvent>
    {
        private readonly ILogger<ProbableCashAppHandler> _logger;
        private readonly IFlagHelper _flagHelper;

        public ProbableCashAppHandler(
            ILogger<ProbableCashAppHandler> logger,
            IFlagHelper flagHelper
        )
        {
            _logger = logger;
            _flagHelper = flagHelper;
        }

        public async Task Consume(ConsumeContext<ProcessFlagRequestEvent> context)
        {
            var autodecisionComposite = await _flagHelper.GetAutodecisionCompositeData(context.Message.Key);
            if (!_flagHelper.IsValidVersion(context.Message, autodecisionComposite, FlagCode.ProbableCashApp, _logger)) return;
            var response = ProcessFlag(autodecisionComposite);
            await _flagHelper.SendReponseMessage(response);
        }

        public ProcessFlagResponseEvent ProcessFlag(AutodecisionCompositeData autodecisionCompositeData)
        {
            var response = _flagHelper.BuildFlagResponse(FlagCode.ProbableCashApp, autodecisionCompositeData);
            try
            {
                if (autodecisionCompositeData.Application.IsProbableCashApp)
                {
                    response.FlagResult = FlagResultEnum.PendingApproval;
                    response.Message = $"The bank routing number {autodecisionCompositeData.Application.BankRoutingNumber} comes from a CashApp";
                    return response;
                }

                response.FlagResult = FlagResultEnum.Processed;
                return response;
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"FlagCode: {FlagCode.ProbableCashApp} was not successfully processed for LoanNumber: {autodecisionCompositeData.Application.LoanNumber} | Error: {e.Message}");

                response.Message = e.Message;
                response.FlagResult = FlagResultEnum.Error;
                return response;
            }
        }
    }
}
