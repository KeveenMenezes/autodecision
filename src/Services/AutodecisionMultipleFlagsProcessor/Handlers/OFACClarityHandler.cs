using AutodecisionCore.Contracts.Constants;
using AutodecisionCore.Contracts.Enums;
using AutodecisionCore.Contracts.Messages;
using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionMultipleFlagsProcessor.Extensions;
using AutodecisionMultipleFlagsProcessor.Services;
using AutodecisionMultipleFlagsProcessor.Services.Interfaces;
using MassTransit;

namespace AutodecisionMultipleFlagsProcessor.Handlers
{
    public class OFACClarityHandler : IConsumer<ProcessFlagRequestEvent>
    {
        private readonly ILogger<OFACClarityHandler> _logger;
        private readonly IFlagHelper _flagHelper;
        private readonly ICustomerInfo _customerInfo;

        public OFACClarityHandler(ILogger<OFACClarityHandler> logger,
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

            if (!_flagHelper.IsValidVersion(context.Message, autodecisionComposite, FlagCode.ClarityOFACHIT, _logger))
                return;

            var response = ProcessFlag(autodecisionComposite);
            response.Reason = context.Message.Reason;
            await _flagHelper.SendReponseMessage(response);
        }
        public ProcessFlagResponseEvent ProcessFlag(AutodecisionCompositeData autodecisionCompositeData)
        {
            var response = new ProcessFlagResponseEvent();
            try
            {
                response = _flagHelper.BuildFlagResponse(FlagCode.ClarityOFACHIT, autodecisionCompositeData, FlagResultEnum.Processed);
                if (autodecisionCompositeData.Clarity is null)
                {
                    _logger.LogWarning($"Clarity data is null for customerID: {autodecisionCompositeData.Customer.Id}");
                    response.FlagResult = FlagResultEnum.Ignored;
                    response.Message = "Customer not found on clarity.";
                    return response;
                }

                if (autodecisionCompositeData.Clarity.OFACHit)
                {
                    _logger.LogInformation($"OFAC Clarity flag raised for customerID: {autodecisionCompositeData.Customer.Id}");
                    response.FlagResult = FlagResultEnum.PendingApproval;
                    response.Message = "This customer appears on the OFAC sanctions list.";
                    _flagHelper.RaiseFlag(response, "This customer appears on the OFAC sanctions list.");
                    return response;
                }

                _logger.LogInformation($"No OFAC Clarity hit for customerID: {autodecisionCompositeData.Customer.Id}");
                response.FlagResult = FlagResultEnum.Approved;
                response.Message = "No match found on the OFAC sanctions list for this customer.";
                return response;
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error processing OFAC Clarity flag for customerID: {autodecisionCompositeData.Customer.Id}");
                response.FlagResult = FlagResultEnum.Error;
                response.Message = "Error processing OFAC Clarity flag.";
                return response;
            }
        }
    }
}
