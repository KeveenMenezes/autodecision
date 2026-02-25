using AutodecisionCore.Contracts.Enums;
using AutodecisionCore.Contracts.Messages;
using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionMultipleFlagsProcessor.Extensions;
using AutodecisionMultipleFlagsProcessor.Services;
using AutodecisionMultipleFlagsProcessor.Services.Interfaces;
using MassTransit;

namespace AutodecisionMultipleFlagsProcessor.Handlers
{
    public class OneLoanPerSSNHandler : IConsumer<ProcessFlagRequestEvent>
    {
        private readonly ILogger<OneLoanPerSSNHandler> _logger;
        private readonly IFlagHelper _flagHelper;
        private readonly ICustomerInfo _customerInfo;

        public OneLoanPerSSNHandler(ILogger<OneLoanPerSSNHandler> logger, 
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

			var response = ProcessFlag(autodecisionComposite);
			response.Reason = context.Message.Reason;

			await _flagHelper.SendReponseMessage(response);
        }

        public ProcessFlagResponseEvent ProcessFlag(AutodecisionCompositeData autodecisionCompositeData)
        {
            var response = _flagHelper.BuildFlagResponse(FlagCode.OneLoanPerSSN, autodecisionCompositeData, FlagResultEnum.Processed);

            try
            {
                var loan_number = _customerInfo.GetOtherOpenLoan(autodecisionCompositeData.Application.Id, autodecisionCompositeData.Application.CustomerId, autodecisionCompositeData.Application.Type).Result;

                if (!string.IsNullOrEmpty(loan_number?.Data))
                {
                    _flagHelper.RaiseFlag(response, $"Another application found for this customer: #{loan_number}");

                    if (autodecisionCompositeData.WhiteList == null)
                    {
                        response.FlagResult = FlagResultEnum.AutoDeny;
                        response.Message = "Application Denied";
                        return response;
                    }
                }

                return response;

            }
            catch (Exception e)
            {
                _logger.LogError(e, $"FlagCode: {FlagCode.OneLoanPerSSN} was not successfully processed for LoanNumber: {autodecisionCompositeData.Application.LoanNumber} | Error: {e.Message}");

                response.Message = e.Message;
                response.FlagResult = FlagResultEnum.Error;
                return response;
            }
        }
    }
}
