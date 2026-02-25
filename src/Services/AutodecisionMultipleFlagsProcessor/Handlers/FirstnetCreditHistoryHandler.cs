using AutodecisionCore.Contracts.Messages;
using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionCore.Contracts.Constants;
using AutodecisionCore.Contracts.Enums;
using AutodecisionMultipleFlagsProcessor.Services;
using AutodecisionMultipleFlagsProcessor.Services.Interfaces;
using MassTransit;
using AutodecisionMultipleFlagsProcessor.Extensions;

namespace AutodecisionMultipleFlagsProcessor.Handlers
{
    public class FirstnetCreditHistoryHandler : IConsumer<ProcessFlagRequestEvent>
    {
        private readonly ILogger<FirstnetCreditHistoryHandler> _logger;
        private readonly IFlagHelper _flagHelper;
        private readonly ICustomerInfo _customerInfo;

        public FirstnetCreditHistoryHandler(ILogger<FirstnetCreditHistoryHandler> logger, IFlagHelper flagHelper, ICustomerInfo customerInfo)
        {
            _logger = logger;
            _flagHelper = flagHelper;
            _customerInfo = customerInfo;
        }

        public async Task Consume(ConsumeContext<ProcessFlagRequestEvent> context)
        {
            var autodecisionComposite = await _flagHelper.GetAutodecisionCompositeData(context.Message.Key);

			if (!_flagHelper.IsValidVersion(context.Message, autodecisionComposite, FlagCode.FirstnetCreditHistory, _logger))
				return;

			var response = ProcessFlag(autodecisionComposite);
			response.Reason = context.Message.Reason;
            await _flagHelper.SendReponseMessage(response);
        }

        public ProcessFlagResponseEvent ProcessFlag(AutodecisionCompositeData autodecisionCompositeData)
        {
            var response = _flagHelper.BuildFlagResponse(FlagCode.FirstnetCreditHistory, autodecisionCompositeData);
			try
            {

                if (autodecisionCompositeData.Application.Type != ApplicationType.Refi)
                {
                    response.FlagResult = FlagResultEnum.Ignored;
                    return response;
                }

                if (IsLastApplicationFirstNetPassThru(autodecisionCompositeData))
                {
                    response.FlagResult = FlagResultEnum.Ignored;
                    return response;
                }

                if (_customerInfo.CheckFirstnetCredit(autodecisionCompositeData.Customer.Id, 20).Result)
                {
                    response.FlagResult = FlagResultEnum.PendingApproval;
                    response.Message = "Firstnet History Credit Check Required.";
                    return response;
                }

                response.FlagResult = FlagResultEnum.Processed;
                return response;
            }
            catch (Exception e)
            {
				_logger.LogError(e, $"FlagCode: {FlagCode.FirstnetCreditHistory} was not successfully processed for LoanNumber: {autodecisionCompositeData.Application.LoanNumber} | Error: {e.Message}");

				response.Message = e.Message;
                response.FlagResult = FlagResultEnum.Error;
                return response;
            }
        }

        private Boolean IsLastApplicationFirstNetPassThru(AutodecisionCompositeData autodecisionCompositeData)
        {
            List<LastApplication> lastApplications = autodecisionCompositeData.LastApplications;
            if (lastApplications != null)
            {
                lastApplications.Sort((a, b) => b.CreatedAt.CompareTo(a.CreatedAt));
            }
            if (lastApplications[0].ReconciliationSystem == "firstnetpassthru")
            {
                return true;
            }
            return false;
        }

    }
}