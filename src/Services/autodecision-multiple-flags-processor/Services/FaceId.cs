using AutodecisionMultipleFlagsProcessor.DTOs;
using AutodecisionMultipleFlagsProcessor.Services.Interfaces;
using BMGMoney.SDK.V2.Http;
using Newtonsoft.Json;

namespace AutodecisionMultipleFlagsProcessor.Services
{
    public class FaceId : IFaceId
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpService _httpService;
        public FaceId(IConfiguration configuration, IHttpService httpService)
        {
            _configuration = configuration;
            _httpService = httpService;
        }

        public async Task<CustomerFaceIdDTO> GetCustomerFaceId(int customer_id)
        {
            string url = _configuration["Apis:FaceIdApi"] + $"/Customer/{customer_id}/details/fraud";

            var result = await _httpService.GetAsync(url);

            var response = new CustomerFaceIdDTO();

            if (result.Success)
                response = JsonConvert.DeserializeObject<CustomerFaceIdDTO>(result.Response);
            else
                response.Response = new Response{
                    Enrollments = new List<Enrollment>()
                };

            return response;
        }

    }
}
