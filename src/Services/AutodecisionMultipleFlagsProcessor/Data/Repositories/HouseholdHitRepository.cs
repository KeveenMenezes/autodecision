using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionMultipleFlagsProcessor.Data.Repositories.Interfaces;
using AutodecisionMultipleFlagsProcessor.DTOs;
using AutodecisionMultipleFlagsProcessor.Services.Interfaces;
using AutodecisionMultipleFlagsProcessor.Utility;
using System.Data;

namespace AutodecisionMultipleFlagsProcessor.Data.Repositories
{
    public class HouseholdHitRepository : IHouseholdHitRepository
    {
        private readonly ICustomerInfo _customerInfo;

        public HouseholdHitRepository(ICustomerInfo customerInfo)
        {
            _customerInfo = customerInfo;
        }

        private async Task<List<SimilarAddressDto>> GetCustomersWithSameAddressAsync(Customer customer)
        {
            var similarAddressDto = new SimilarAddressDto(customer.Id, string.Empty, customer.StreetAddress, customer.UnitNumber, customer.CityName, customer.StateAbbreviation, customer.ZipCode, 0, 0);
            var customersWithSameAddress = await _customerInfo.CheckCustomersWithSameAddressAsync(similarAddressDto);
            var similarAddressList = new List<SimilarAddressDto>();

            foreach (var item in customersWithSameAddress)
            {
                string similarAddress = item.StreetAddress + " " + item.UnitNumber;
                similarAddress = similarAddress.Replace(" ", "");
                item.Similarity = 0;
                similarAddressList.Add(item);
            }

            return similarAddressList.OrderByDescending(p => p.Similarity).ToList();
        }

        public async Task<List<SimilarAddressDto>> GetSimilarAddressListAsync(Customer customer, decimal max_similarity)
        {
            var customersWithSameAddress = await GetCustomersWithSameAddressAsync(customer);
            if (customersWithSameAddress?.Count > 0)
                return customersWithSameAddress;

            string address = customer.StreetAddress + " " + customer.UnitNumber;
            int levenshteinDistance = Math.Max(-1 * (int)Math.Round((max_similarity - (1.0m) * address.Length)), 1);

            var customersWithSimilarAddress = await _customerInfo.GetSimilarAddressListForCustomerAsync(customer.Id, levenshteinDistance);

            var similarAddressList = new List<SimilarAddressDto>();
            var customerAddress = address.Replace(" ", "");

            foreach (var item in customersWithSimilarAddress)
            {
                var customerUnitNumber = string.IsNullOrEmpty(customer.UnitNumber) ? null : customer.UnitNumber.Trim();
                var itemUnitNumber = string.IsNullOrEmpty(item.UnitNumber) ? null : item.UnitNumber.Trim();

                if (customerUnitNumber == itemUnitNumber)
                {
                    string similarAddress = item.StreetAddress + " " + item.UnitNumber;
                    similarAddress = similarAddress.Replace(" ", "");
                    item.Similarity = LevenshteinDistance.CalculateSimilarity(customerAddress.ToLower(), similarAddress.ToLower());
                    if (item.Similarity >= max_similarity)
                        similarAddressList.Add(item);
                }
            }

            return similarAddressList.OrderByDescending(p => p.Similarity).ToList();
        }
    }
}