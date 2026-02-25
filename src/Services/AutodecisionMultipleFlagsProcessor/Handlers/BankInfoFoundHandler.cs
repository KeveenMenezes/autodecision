using AutodecisionCore.Contracts.Messages;
using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionMultipleFlagsProcessor.Services.Interfaces;
using AutodecisionMultipleFlagsProcessor.Services;
using MassTransit;
using AutodecisionCore.Contracts.Enums;
using AutodecisionMultipleFlagsProcessor.DTOs;
using AutodecisionMultipleFlagsProcessor.Extensions;

namespace AutodecisionMultipleFlagsProcessor.Handlers
{
    public class BankInfoFoundHandler : IConsumer<ProcessFlagRequestEvent>
    {

        private readonly ILogger<BankInfoFoundHandler> _logger;
        private readonly IFlagHelper _flagHelper;
        private readonly ICustomerInfo _customerInfo;

        public BankInfoFoundHandler(ILogger<BankInfoFoundHandler> logger,
            IFlagHelper flagHelper,
            ICustomerInfo customerInfo
            )
        {
            _logger = logger;
            _flagHelper = flagHelper;
            _customerInfo = customerInfo;
        }

        public async Task Consume(ConsumeContext<ProcessFlagRequestEvent> context)
        {
            var autodecisionComposite = await _flagHelper.GetAutodecisionCompositeData(context.Message.Key);

			if (!_flagHelper.IsValidVersion(context.Message, autodecisionComposite, FlagCode.BankInfoFound, _logger))
				return;

			var response = ProcessFlag(autodecisionComposite);
			response.Reason = context.Message.Reason;
			await _flagHelper.SendReponseMessage(response);
        }

        public ProcessFlagResponseEvent ProcessFlag(AutodecisionCompositeData autodecisionCompositeData)
        {
            var response = _flagHelper.BuildFlagResponse(FlagCode.BankInfoFound, autodecisionCompositeData, FlagResultEnum.Processed);

			try
            {
                var similar_bank_list = _customerInfo.GetApplicationsWithSameBankInfo(autodecisionCompositeData.Application.CustomerId, autodecisionCompositeData.Application.BankRoutingNumber, autodecisionCompositeData.Application.BankAccountNumber).Result;

                if (similar_bank_list.Data.Count > 0)
                {
                    response.Message = GetDescription(similar_bank_list.Data, 10);
                    response.FlagResult = FlagResultEnum.PendingApproval;
                }

                return response;
            }
            catch (Exception e)
            {
				_logger.LogError(e, $"FlagCode: {FlagCode.BankInfoFound} was not successfully processed for LoanNumber: {autodecisionCompositeData.Application.LoanNumber} | Error: {e.Message}");

				response.Message = e.Message;
                response.FlagResult = FlagResultEnum.Error;
                return response;
            }
        }

        private string GetDescription(List<ApplicationsWithSameBankInfo> similar_bank_list, int count)
        {
            string description = $"Bank Information critical | Same bank info found for other {similar_bank_list.Count} customer(s): ";
            if (similar_bank_list.Count > count)
                description += string.Join(",", similar_bank_list.Take(count).Select(p => $"#{p.LoanNumber}")) + ", ...";
            else
                description += string.Join(",", similar_bank_list.Select(p => $"#{p.LoanNumber}"));

            return description;
        }

    }
}
