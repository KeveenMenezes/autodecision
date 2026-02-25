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
    public class PhoneValidationHandler : IConsumer<ProcessFlagRequestEvent>
    {
        private readonly ILogger<PhoneValidationHandler> _logger;
        private readonly IFlagHelper _flagHelper;
        private readonly ICustomerInfo _customerInfo;

        public PhoneValidationHandler(ILogger<PhoneValidationHandler> logger, IFlagHelper flagHelper, ICustomerInfo customerInfo)
        {
            _logger = logger;
            _flagHelper = flagHelper;
            _customerInfo = customerInfo;
        }

        public async Task Consume(ConsumeContext<ProcessFlagRequestEvent> context)
        {
            var autodecisionComposite = await _flagHelper.GetAutodecisionCompositeData(context.Message.Key);

            if (!_flagHelper.IsValidVersion(context.Message, autodecisionComposite, FlagCode.PhoneValidation, _logger))
                return;

			var response = ProcessFlag(autodecisionComposite);
			response.Reason = context.Message.Reason;
            await _flagHelper.SendReponseMessage(response);
        }

        public ProcessFlagResponseEvent ProcessFlag(AutodecisionCompositeData autodecisionCompositeData)
        {
            var response = _flagHelper.BuildFlagResponse(FlagCode.PhoneValidation, autodecisionCompositeData);

            try
            {
                if (autodecisionCompositeData.Application.Type != ApplicationType.NewLoan)
                {
                    response.FlagResult = FlagResultEnum.Ignored;
                    return response;
                }

                var otherCustomersSamePhoneNumber = _customerInfo.GetCustomersWithSamePhone(
                    autodecisionCompositeData.Customer.Id,
                    autodecisionCompositeData.Customer.PhoneNumber,
                    autodecisionCompositeData.Customer.SecondaryPhoneNumber,
                    autodecisionCompositeData.Customer.WorkPhoneNumber
                );

                if (otherCustomersSamePhoneNumber.Result.Count > 0)
                {
                    _flagHelper.RaiseFlag(response, GetDescription(otherCustomersSamePhoneNumber.Result, 10));
                    return response;
                }

                response.FlagResult = FlagResultEnum.Processed;
                return response;
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"FlagCode: {FlagCode.PhoneValidation} was not successfully processed for LoanNumber: {autodecisionCompositeData.Application.LoanNumber} | Error: {e.Message}");

                response.Message = e.Message;
                response.FlagResult = FlagResultEnum.Error;
                return response;
            }
        }

        private string GetDescription(List<DTOs.SimilarPhoneDataDTO> similar_phones, int count)
        {
            string description = $"Phone number already in use by other {similar_phones.Count} customer(s): ";

            if (similar_phones.Count > count)
                description += string.Join(",", similar_phones.Take(count).Select(p => $"#{p.LoanNumber}")) + ", ...";
            else
                description += string.Join(",", similar_phones.Select(p => $"#{p.LoanNumber}"));

            return description;
        }

    }
}