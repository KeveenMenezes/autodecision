using AutodecisionCore.Contracts.Messages;
using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionCore.Contracts.Enums;
using AutodecisionMultipleFlagsProcessor.Services;
using MassTransit;
using AutodecisionMultipleFlagsProcessor.Extensions;

namespace AutodecisionMultipleFlagsProcessor.Handlers
{
    public class FoundOnBlockListHandler : IConsumer<ProcessFlagRequestEvent>
    {
        private readonly ILogger<FoundOnBlockListHandler> _logger;
        private readonly IFlagHelper _flagHelper;

        public FoundOnBlockListHandler(ILogger<FoundOnBlockListHandler> logger, IFlagHelper flagHelper)
        {
            _logger = logger;
            _flagHelper = flagHelper;
        }

        public async Task Consume(ConsumeContext<ProcessFlagRequestEvent> context)
        {
            var autodecisionComposite = await _flagHelper.GetAutodecisionCompositeData(context.Message.Key);

			if (!_flagHelper.IsValidVersion(context.Message, autodecisionComposite, FlagCode.FoundOnBlockList, _logger))
				return;

			var response = ProcessFlag(autodecisionComposite);
			response.Reason = context.Message.Reason;
            await _flagHelper.SendReponseMessage(response);
        }

        public ProcessFlagResponseEvent ProcessFlag(AutodecisionCompositeData autodecisionCompositeData)
        {
            var response = _flagHelper.BuildFlagResponse(FlagCode.FoundOnBlockList, autodecisionCompositeData);

			try
            {
                if (autodecisionCompositeData.BlockList is not null)
                {
                    if (!string.IsNullOrEmpty(autodecisionCompositeData.BlockList.Reason))
                    {
                        response.FlagResult = FlagResultEnum.PendingApproval;
                        response.Message = $"Customer found on Alert List. Reason: {autodecisionCompositeData.BlockList.Reason} | Note: {autodecisionCompositeData.BlockList.CreatedNote}  | Created at: {autodecisionCompositeData.BlockList.CreatedAt.Value.ToString("MM/dd/yyyy")} ";
                        return response;
                    }
                }

                response.FlagResult = FlagResultEnum.Processed;
                return response;
            }
            catch (Exception e)
            {
				_logger.LogError(e, $"FlagCode: {FlagCode.FoundOnBlockList} was not successfully processed for LoanNumber: {autodecisionCompositeData.Application.LoanNumber} | Error: {e.Message}");

				response.Message = e.Message;
                response.FlagResult = FlagResultEnum.Error;
                return response;
            }
        }
    }
}