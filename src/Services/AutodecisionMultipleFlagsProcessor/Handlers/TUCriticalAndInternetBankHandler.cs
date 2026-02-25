using AutodecisionCore.Contracts.Constants;
using AutodecisionCore.Contracts.Enums;
using AutodecisionCore.Contracts.Messages;
using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionMultipleFlagsProcessor.Extensions;
using AutodecisionMultipleFlagsProcessor.Services;
using MassTransit;

namespace AutodecisionMultipleFlagsProcessor.Handlers
{
    public class TUCriticalAndInternetBankHandler : IConsumer<ProcessFlagRequestEvent>
    {
        private readonly ILogger<TUCriticalAndInternetBankHandler> _logger;
        private readonly IFlagHelper _flagHelper;

        public TUCriticalAndInternetBankHandler(ILogger<TUCriticalAndInternetBankHandler> logger, IFlagHelper flagHelper)
        {
            _logger = logger;
            _flagHelper = flagHelper;
        }

        public async Task Consume(ConsumeContext<ProcessFlagRequestEvent> context)
        {
            var autodecisionComposite = await _flagHelper.GetAutodecisionCompositeData(context.Message.Key);

            if (!_flagHelper.IsValidVersion(context.Message, autodecisionComposite, FlagCode.TUCriticalAndInternetBank, _logger))
                return;

			var response = ProcessFlag(autodecisionComposite);
			response.Reason = context.Message.Reason;
            await _flagHelper.SendReponseMessage(response);
        }

        public ProcessFlagResponseEvent ProcessFlag(AutodecisionCompositeData autodecisionCompositeData)
        {
            var response = _flagHelper.BuildFlagResponse(FlagCode.TUCriticalAndInternetBank, autodecisionCompositeData);

            try
            {
                if (autodecisionCompositeData.Application.Type == ApplicationType.Refi)
                {
                    response.FlagResult = FlagResultEnum.Ignored;
                    return response;
                }

                if (autodecisionCompositeData.Application.FundingMethod == ApplicationFundingMethod.Check)
                {
                    response.FlagResult = FlagResultEnum.Ignored;
                    return response;
                }

                if (autodecisionCompositeData.TransunionResult is not null)
                {
                    if (autodecisionCompositeData.TransunionResult?.Score >= 30)
                    {
						response.FlagResult = FlagResultEnum.Ignored;
                        return response;
                    }
                }

                if (autodecisionCompositeData.Application.ProductId == ApplicationProductId.ProductAtWork)
                {
                    response.FlagResult = FlagResultEnum.Ignored;
                    return response;
                }

                if (!autodecisionCompositeData.Application.HighRisk)
                {
                    response.FlagResult = FlagResultEnum.Ignored;
                    return response;
                }

                if (autodecisionCompositeData.Application.HighRisk)
                {
                    if (autodecisionCompositeData.WhiteList is null && autodecisionCompositeData.Application.TurndownActive)
                    {
                        response.FlagResult = FlagResultEnum.AutoDeny;
                        return response;
                    }

                    if (string.IsNullOrEmpty(autodecisionCompositeData.WhiteList?.Reason) && autodecisionCompositeData.Application.TurndownActive)
                    {
                        response.FlagResult = FlagResultEnum.AutoDeny;
                        return response;
                    }

                    _flagHelper.RaiseFlag(response, "Traunsunion score critical and high risk bank");
                    return response;
                }

                response.FlagResult = FlagResultEnum.Processed;
                return response;

            }
            catch (Exception e)
            {
                _logger.LogError(e, $"FlagCode: {FlagCode.TUCriticalAndInternetBank} was not successfully processed for LoanNumber: {autodecisionCompositeData.Application.LoanNumber} | Error: {e.Message}");

                response.Message = e.Message;
                response.FlagResult = FlagResultEnum.Error;
                return response;
            }
        }
    }
}