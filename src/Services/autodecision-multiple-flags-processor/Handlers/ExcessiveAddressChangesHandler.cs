using AutodecisionCore.Contracts.Messages;
using AutodecisionCore.Contracts.Constants;
using AutodecisionCore.Contracts.Enums;
using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionMultipleFlagsProcessor.Services;
using MassTransit;
using AutodecisionMultipleFlagsProcessor.Extensions;

namespace AutodecisionMultipleFlagsProcessor.Handlers
{
    public class ExcessiveAddressChangesHandler : IConsumer<ProcessFlagRequestEvent>
    {
        private readonly ILogger<ExcessiveAddressChangesHandler> _logger;
        private readonly IFlagHelper _flagHelper;

        public ExcessiveAddressChangesHandler(ILogger<ExcessiveAddressChangesHandler> logger, IFlagHelper flagHelper)
        {
            _logger = logger;
            _flagHelper = flagHelper;
        }

        public async Task Consume(ConsumeContext<ProcessFlagRequestEvent> context)
        {
            var autodecisionComposite = await _flagHelper.GetAutodecisionCompositeData(context.Message.Key);

			if (!_flagHelper.IsValidVersion(context.Message, autodecisionComposite, FlagCode.ExcessiveAddressChanges, _logger))
				return;

			var response = ProcessFlag(autodecisionComposite);
			response.Reason = context.Message.Reason;
            await _flagHelper.SendReponseMessage(response);
        }

        public ProcessFlagResponseEvent ProcessFlag(AutodecisionCompositeData autodecisionCompositeData)
        {
            var response = _flagHelper.BuildFlagResponse(FlagCode.ExcessiveAddressChanges, autodecisionCompositeData);

			try
            {
                if (autodecisionCompositeData.Application.Type == ApplicationType.Refi)
                {
                    response.FlagResult = FlagResultEnum.Ignored;
                    return response;
                }

                if (autodecisionCompositeData.FactorTrust.AddressChangesLastTwoYears == null)
                {
                    response.FlagResult = FlagResultEnum.Ignored;
                    response.Message = "FactorTrust response not found.";
                    return response;
                }

                var max_value = 3;

                if (autodecisionCompositeData.FactorTrust.AddressChangesLastTwoYears > max_value)
                {
                    if (string.IsNullOrEmpty(autodecisionCompositeData.WhiteList.Reason))
                    {
                        response.FlagResult = FlagResultEnum.AutoDeny;
                        return response;
                    }
                    response.FlagResult = FlagResultEnum.PendingApproval;
                    response.Message = $"Excessive changes in address greater than {max_value} based on Factor Trust. Changed: {autodecisionCompositeData.FactorTrust.AddressChangesLastTwoYears}";
                    return response;
                }

                response.FlagResult = FlagResultEnum.Processed;
                return response;
            }
            catch (Exception e)
            {
				_logger.LogError(e, $"FlagCode: {FlagCode.ExcessiveAddressChanges} was not successfully processed for LoanNumber: {autodecisionCompositeData.Application.LoanNumber} | Error: {e.Message}");

				response.Message = e.Message;
                response.FlagResult = FlagResultEnum.Error;
                return response;
            }
        }
    }
}