using AutodecisionCore.Contracts.Enums;
using AutodecisionCore.Contracts.Messages;
using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionMultipleFlagsProcessor.Extensions;
using AutodecisionMultipleFlagsProcessor.Services;
using AutodecisionMultipleFlagsProcessor.Services.Interfaces;
using MassTransit;
using System.Text.RegularExpressions;

namespace AutodecisionMultipleFlagsProcessor.Handlers
{
    public class RoutingNumberHandler : IConsumer<ProcessFlagRequestEvent>
    {
        private readonly ILogger<RoutingNumberHandler> _logger;
        private readonly IFlagHelper _flagHelper;
        private readonly ICustomerInfo _customerInfo;

        public RoutingNumberHandler(ILogger<RoutingNumberHandler> logger, IFlagHelper flagHelper, ICustomerInfo customerInfo)
        {
            _logger = logger;
            _flagHelper = flagHelper;
            _customerInfo = customerInfo;
        }

        public async Task Consume(ConsumeContext<ProcessFlagRequestEvent> context)
        {
            var autodecisionComposite = await _flagHelper.GetAutodecisionCompositeData(context.Message.Key);

            if (!_flagHelper.IsValidVersion(context.Message, autodecisionComposite, FlagCode.RoutingNumberVerification, _logger))
                return;

			var response = ProcessFlag(autodecisionComposite);
			response.Reason = context.Message.Reason;
            await _flagHelper.SendReponseMessage(response);
        }

        public ProcessFlagResponseEvent ProcessFlag(AutodecisionCompositeData autodecisionCompositeData)
        {
            var response = _flagHelper.BuildFlagResponse(FlagCode.RoutingNumberVerification, autodecisionCompositeData);

            try
            {
                var routingNumberResult = IsRoutingNumberValid(autodecisionCompositeData.Application.BankRoutingNumber);
                var routingNumberHasValue = routingNumberResult != null;

                if (routingNumberHasValue && routingNumberResult.GetValueOrDefault("success") != null && (bool)routingNumberResult.GetValueOrDefault("success"))
                {
                    response.FlagResult = FlagResultEnum.Processed;
                    return response;
                }

                _flagHelper.RaiseFlag(response, (string)routingNumberResult.GetValueOrDefault("message"));

                return response;
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"FlagCode: {FlagCode.RoutingNumberVerification} was not successfully processed for LoanNumber: {autodecisionCompositeData.Application.LoanNumber} | Error: {e.Message}");

                response.Message = e.Message;
                response.FlagResult = FlagResultEnum.Error;
                return response;
            }
        }

        private Dictionary<string, object> IsRoutingNumberValid(string routingNumber)
        {
            if (string.IsNullOrEmpty(routingNumber))
                return new Dictionary<string, object> {
                    { "success", false },
                    { "message", "The routing number was not informed"}
                };

            var regex = new Regex("^[0-9]{9}");
            if (!regex.IsMatch(routingNumber))
                return new Dictionary<string, object> {
                    { "success", false },
                    { "message", "The routing number must be a numeric nine digits long"}
                };

            if (!CalculateChecksum(routingNumber)) return new Dictionary<string, object> {
                    { "success", false },
                    { "message", "The routing number is not valid"}
                };

            var exists = _customerInfo.CheckIfRoutingNumberAlreadyExists(routingNumber).Result;
            return new Dictionary<string, object> {
                    { "success", exists },
                    { "message", exists ? default : "The routing number was not founded"}
                };
        }

        private static bool CalculateChecksum(string routingNumber)
        {
            var routingNumberArray = System.Array.ConvertAll(routingNumber.ToCharArray(), s => int.Parse(s.ToString()));
            var checkSum = 3 * (routingNumberArray[0] + routingNumberArray[3] + routingNumberArray[6]) +
                           7 * (routingNumberArray[1] + routingNumberArray[4] + routingNumberArray[7]) +
                           routingNumberArray[2] + routingNumberArray[5] + routingNumberArray[8];

            if (checkSum <= 0) return false;

            return checkSum % 10 == 0;
        }
    }
}
