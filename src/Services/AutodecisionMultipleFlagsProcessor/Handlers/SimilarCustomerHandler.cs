using AutodecisionCore.Contracts.Enums;
using AutodecisionCore.Contracts.Messages;
using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionMultipleFlagsProcessor.DTOs;
using AutodecisionMultipleFlagsProcessor.Extensions;
using AutodecisionMultipleFlagsProcessor.Services;
using AutodecisionMultipleFlagsProcessor.Services.Interfaces;
using MassTransit;

namespace AutodecisionMultipleFlagsProcessor.Handlers
{
    public class SimilarCustomerHandler : IConsumer<ProcessFlagRequestEvent>
    {
        private readonly ILogger<SimilarCustomerHandler> _logger;
        private readonly IFlagHelper _flagHelper;
        private readonly ICustomerInfo _customerInfo;

        public SimilarCustomerHandler(ILogger<SimilarCustomerHandler> logger,
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

            if (!_flagHelper.IsValidVersion(context.Message, autodecisionComposite, FlagCode.SimilarCustomer, _logger))
                return;

			var response = ProcessFlag(autodecisionComposite);
			response.Reason = context.Message.Reason;
            await _flagHelper.SendReponseMessage(response);
        }

        public ProcessFlagResponseEvent ProcessFlag(AutodecisionCompositeData autodecisionCompositeData)
        {
            var response = _flagHelper.BuildFlagResponse(FlagCode.SimilarCustomer, autodecisionCompositeData, FlagResultEnum.Processed);

            try
            {
                var similar_customers = _customerInfo.GetSimilarCustomers(autodecisionCompositeData.Customer.Id).Result;

                if (similar_customers.Count > 0)
                    _flagHelper.RaiseFlag(response, GetDescription(similar_customers, 3));

                return response;
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"FlagCode: {FlagCode.SimilarCustomer} was not successfully processed for LoanNumber: {autodecisionCompositeData.Application.LoanNumber} | Error: {e.Message}");

                response.Message = e.Message;
                response.FlagResult = FlagResultEnum.Error;
                return response;
            }
        }

        private string GetDescription(List<SimilarCustomerDto> similarCustomers, int count)
        {
            string description = $"{similarCustomers.Count} customer(s) found with similar name and SSN";

            if (similarCustomers.Count > count)
            {
                description += $" - Top {count} ocurrences: ";
                similarCustomers = similarCustomers.Take(count).ToList();
            }

            var list = new List<string>();
            foreach (var customer in similarCustomers)
                list.Add($" Customer: {customer.first_name} {customer.last_name} - SSN: {customer.ssn} - #{customer.loan_number} ");

            description += string.Join(", ", list);
            return description;
        }

    }
}
