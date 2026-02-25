using AutodecisionCore.Contracts.Messages;
using AutodecisionCore.Contracts.Constants;
using AutodecisionCore.Contracts.Enums;
using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionMultipleFlagsProcessor.Services;
using MassTransit;
using AutodecisionMultipleFlagsProcessor.Extensions;

namespace AutodecisionMultipleFlagsProcessor.Handlers
{
    public class HighestCreditLimitHandler : IConsumer<ProcessFlagRequestEvent>
    {
        private readonly ILogger<HighestCreditLimitHandler> _logger;
        private readonly IFlagHelper _flagHelper;

        public HighestCreditLimitHandler(ILogger<HighestCreditLimitHandler> logger, IFlagHelper flagHelper)
        {
            _logger = logger;
            _flagHelper = flagHelper;
        }

        public async Task Consume(ConsumeContext<ProcessFlagRequestEvent> context)
        {
            var autodecisionComposite = await _flagHelper.GetAutodecisionCompositeData(context.Message.Key);

            if (!_flagHelper.IsValidVersion(context.Message, autodecisionComposite, FlagCode.HighestCreditLimit, _logger))
                return;

			var response = ProcessFlag(autodecisionComposite);
			response.Reason = context.Message.Reason;
            await _flagHelper.SendReponseMessage(response);
        }

        public ProcessFlagResponseEvent ProcessFlag(AutodecisionCompositeData autodecisionCompositeData)
        {
            var response = _flagHelper.BuildFlagResponse(FlagCode.HighestCreditLimit, autodecisionCompositeData);

            try
            {
                if (autodecisionCompositeData.Application.Type == ApplicationType.Refi)
                {
                    response.FlagResult = FlagResultEnum.Ignored;
                    return response;
                }

                if (autodecisionCompositeData.FactorTrust.BalanceToIncome == null)
                {
                    response.FlagResult = FlagResultEnum.Ignored;
                    response.Message = "FactorTrust response not found.";
                    return response;
                }

                var max_value = 2;

                if (autodecisionCompositeData.FactorTrust.BalanceToIncome > max_value)
                {
                    if (string.IsNullOrEmpty(autodecisionCompositeData.WhiteList.Reason))
                    {
                        response.FlagResult = FlagResultEnum.AutoDeny;
                        return response;
                    }
                    response.FlagResult = FlagResultEnum.PendingApproval;
                    response.Message = $"Highest Credit Rule based on Factor Trust greater than {max_value}. Current: {autodecisionCompositeData.FactorTrust.BalanceToIncome}";
                    return response;
                }

                response.FlagResult = FlagResultEnum.Processed;
                return response;
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"FlagCode: {FlagCode.HighestCreditLimit} was not successfully processed for LoanNumber: {autodecisionCompositeData.Application.LoanNumber} | Error: {e.Message}");

                response.Message = e.Message;
                response.FlagResult = FlagResultEnum.Error;
                return response;
            }
        }
    }
}