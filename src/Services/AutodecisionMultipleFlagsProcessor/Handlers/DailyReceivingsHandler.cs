using AutodecisionCore.Contracts.Messages;
using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionCore.Contracts.Enums;
using AutodecisionMultipleFlagsProcessor.Services;
using AutodecisionMultipleFlagsProcessor.Services.Interfaces;
using MassTransit;
using AutodecisionMultipleFlagsProcessor.Extensions;

namespace AutodecisionMultipleFlagsProcessor.Handlers
{
    public class DailyReceivingsHandler : IConsumer<ProcessFlagRequestEvent>
    {
        private readonly ILogger<DailyReceivingsHandler> _logger;
        private readonly IFlagHelper _flagHelper;
        private readonly IOpenBankingService _openBankingService;

        public DailyReceivingsHandler(ILogger<DailyReceivingsHandler> logger, IFlagHelper flagHelper, IOpenBankingService openBankingService)
        {
            _logger = logger;
            _flagHelper = flagHelper;
            _openBankingService = openBankingService;
        }

        public async Task Consume(ConsumeContext<ProcessFlagRequestEvent> context)
        {
            var autodecisionComposite = await _flagHelper.GetAutodecisionCompositeData(context.Message.Key);

            if (!_flagHelper.IsValidVersion(context.Message, autodecisionComposite, FlagCode.DailyReceivings, _logger))
                return;

            var response = ProcessFlag(autodecisionComposite);
            response.Reason = context.Message.Reason;
            await _flagHelper.SendReponseMessage(response);
        }

        public ProcessFlagResponseEvent ProcessFlag(AutodecisionCompositeData autodecisionCompositeData)
        {
            var response = _flagHelper.BuildFlagResponse(FlagCode.DailyReceivings, autodecisionCompositeData);

            try
            {
                var dailyReceivings = _openBankingService.GetDailyReceivings(autodecisionCompositeData.Customer.Id).Result;
                if (dailyReceivings.Data)
                {
                    response.FlagResult = FlagResultEnum.PendingApproval;
                    response.Message = "Daily pay found";
                    return response;
                }

                response.FlagResult = FlagResultEnum.Processed;
                return response;
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"FlagCode: {FlagCode.DailyReceivings} was not successfully processed for LoanNumber: {autodecisionCompositeData.Application.LoanNumber} | Error: {e.Message}");

                response.Message = e.Message;
                response.FlagResult = FlagResultEnum.Error;
                return response;
            }
        }
    }
}