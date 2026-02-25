using AutodecisionCore.Contracts.Messages;
using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionCore.Contracts.Enums;
using AutodecisionMultipleFlagsProcessor.Services;
using MassTransit;
using AutodecisionMultipleFlagsProcessor.Extensions;

namespace AutodecisionMultipleFlagsProcessor.Handlers 
{
    public class FoundOnWhiteListHandler : IConsumer<ProcessFlagRequestEvent>
    {
        private readonly ILogger<FoundOnWhiteListHandler> _logger;
        private readonly IFlagHelper _flagHelper;

        public FoundOnWhiteListHandler(ILogger<FoundOnWhiteListHandler> logger, IFlagHelper flagHelper)
        {
            _logger = logger;
            _flagHelper = flagHelper;
        }

        public async Task Consume(ConsumeContext<ProcessFlagRequestEvent> context)
        {
            var autodecisionComposite = await _flagHelper.GetAutodecisionCompositeData(context.Message.Key);

			if (!_flagHelper.IsValidVersion(context.Message, autodecisionComposite, FlagCode.FoundOnWhiteList, _logger))
				return;

			var response = ProcessFlag(autodecisionComposite);
			response.Reason = context.Message.Reason;
            await _flagHelper.SendReponseMessage(response);
        }

        public ProcessFlagResponseEvent ProcessFlag(AutodecisionCompositeData autodecisionCompositeData)
        {
            var response = _flagHelper.BuildFlagResponse(FlagCode.FoundOnWhiteList, autodecisionCompositeData);

			try {

                if (autodecisionCompositeData.WhiteList is not null)
                {
                    if (!string.IsNullOrEmpty(autodecisionCompositeData.WhiteList.Reason))
                    {
                        response.FlagResult = FlagResultEnum.PendingApproval;
                        response.Message = $"Customer found on Whitelist. Reason: {autodecisionCompositeData.WhiteList.Reason} | Note: {autodecisionCompositeData.WhiteList.CreatedNote} | Created by: {autodecisionCompositeData.WhiteList.CreatedBy} | Created at: {autodecisionCompositeData.WhiteList.CreatedAt.Value.ToString("MM/dd/yyyy")}";
                        return response;
                    }
                }

                response.FlagResult = FlagResultEnum.Processed;
                return response;
            }
            catch (Exception e)
            {
				_logger.LogError(e, $"FlagCode: {FlagCode.FoundOnWhiteList} was not successfully processed for LoanNumber: {autodecisionCompositeData.Application.LoanNumber} | Error: {e.Message}");

				response.Message = e.Message;
                response.FlagResult = FlagResultEnum.Error;
                return response;
            }
        }
    }
}