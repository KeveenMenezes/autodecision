using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionMultipleFlagsProcessor.Data.Repositories.Interfaces;
using AutodecisionMultipleFlagsProcessor.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace AutodecisionMultipleFlagsProcessor.Controllers
{
    [Route("test")]
    [ApiController]
    public class TestController : BaseController
    {
        private readonly IHouseholdHitRepository _householdHitRepository;

        public TestController(IHouseholdHitRepository householdHitRepository)
        {
            _householdHitRepository = householdHitRepository;
        }

        [HttpPost]
        [Route("householdhit")]
        public async Task<IActionResult> GetSimilarAddressListAsync([FromBody] SimilarAddressDto request)
        {
            var customer = new Customer()
            {
                Id = request.CustomerId,
                StreetAddress = request.StreetAddress,
                ZipCode = request.Zipcode,
                CityName = request.CityName,
                StateAbbreviation = request.StateAbbreviation,
                UnitNumber = request.UnitNumber
            };
            var similarity = request.Similarity == 0m ? 0.94m : request.Similarity;
            return SuccessWithData(await _householdHitRepository.GetSimilarAddressListAsync(customer, similarity));
        }
    }
}
