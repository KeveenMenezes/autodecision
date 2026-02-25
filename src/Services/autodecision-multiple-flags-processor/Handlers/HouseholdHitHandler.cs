using AutodecisionCore.Contracts.Constants;
using AutodecisionCore.Contracts.Enums;
using AutodecisionCore.Contracts.Messages;
using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionMultipleFlagsProcessor.Data.Repositories.Interfaces;
using AutodecisionMultipleFlagsProcessor.DTOs;
using AutodecisionMultipleFlagsProcessor.Extensions;
using AutodecisionMultipleFlagsProcessor.Services;
using AutodecisionMultipleFlagsProcessor.Services.Interfaces;
using MassTransit;

namespace AutodecisionMultipleFlagsProcessor.Handlers
{
    public class HouseholdHitHandler : IConsumer<ProcessFlagRequestEvent>
    {
        private readonly ILogger<HouseholdHitHandler> _logger;
        private readonly IFlagHelper _flagHelper;
        private readonly IHouseholdHitRepository _houseHoldHitRepository;
        private readonly ICustomerInfo _customerInfo;

        public HouseholdHitHandler(ILogger<HouseholdHitHandler> logger, IFlagHelper flagHelper, IHouseholdHitRepository houseHoldHitRepository, ICustomerInfo customerInfo)
        {
            _logger = logger;
            _flagHelper = flagHelper;
            _houseHoldHitRepository = houseHoldHitRepository;
            _customerInfo = customerInfo;
        }

        public async Task Consume(ConsumeContext<ProcessFlagRequestEvent> context)
        {
            var autodecisionComposite = await _flagHelper.GetAutodecisionCompositeData(context.Message.Key);

            if (!_flagHelper.IsValidVersion(context.Message, autodecisionComposite, FlagCode.HouseHoldHit, _logger))
                return;

            var response = await ProcessFlag(autodecisionComposite);
			response.Reason = context.Message.Reason;

			await _flagHelper.SendReponseMessage(response);
        }

        public async Task<ProcessFlagResponseEvent> ProcessFlag(AutodecisionCompositeData autodecisionCompositeData)
        {
            var response = _flagHelper.BuildFlagResponse(FlagCode.HouseHoldHit, autodecisionCompositeData);

            try
            {
                var similarAddressList = await _houseHoldHitRepository.GetSimilarAddressListAsync(autodecisionCompositeData.Customer, 0.94m);
                if (similarAddressList.Count > 0)
                {

                    if (autodecisionCompositeData.Application.StateAbbreviation != "FL" && autodecisionCompositeData.Application.Type == ApplicationType.Refi)
                    {
                        response.Message = $"Refi and Grow only. Customer is not in Florida state. Customer is in {autodecisionCompositeData.Application.StateAbbreviation}";
                        response.FlagResult = FlagResultEnum.Processed;
                        return response;
                    }
                    if (autodecisionCompositeData.Application.Type == ApplicationType.Refi && !await _customerInfo.CheckHasBook(similarAddressList))
                    {
                        response.Message = "NO BOOKED LOAN";
                        response.FlagResult = FlagResultEnum.Processed;
                        return response;
                    }
                    if (similarAddressList.Count == 1 && await _customerInfo.IsWhitelistRelated(autodecisionCompositeData.Customer.Id, similarAddressList.First().CustomerId))
                    {
                        response.Message = $"Flag 182 HouseholdHit is Whitelisted with the customer #{similarAddressList.First().LoanNumber}";
                        response.FlagResult = FlagResultEnum.Processed;
                        return response;
                    }
                    _flagHelper.RaiseFlag(response, GetDescription(similarAddressList, 3));
                    return response;
                }
                response.FlagResult = FlagResultEnum.Processed;
                return response;

            }
            catch (Exception e)
            {
                _logger.LogError(e, $"FlagCode: {FlagCode.HouseHoldHit} was not successfully processed for LoanNumber: {autodecisionCompositeData.Application.LoanNumber} | Error: {e.Message}");

                response.Message = e.Message;
                response.FlagResult = FlagResultEnum.Error;
                return response;
            }
        }

        private string GetDescription(List<SimilarAddressDto> similar_address_list, int count)
        {
            string description = $"Household critical | Similar address found for other {similar_address_list.Count} customer(s)";

            if (similar_address_list.Count > count)
            {
                description += $" - Top {count} ocurrences: ";
                similar_address_list = similar_address_list.Take(count).ToList();
            }

            var list = new List<string>();

            foreach (var address in similar_address_list)
                list.Add($"#{address.LoanNumber} - Address: {address.StreetAddress} {address.UnitNumber} - {address.CityName}, {address.StateAbbreviation} ");

            description += string.Join(", ", list);

            return description;
        }

    }
}
