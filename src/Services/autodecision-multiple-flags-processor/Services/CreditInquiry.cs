using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionMultipleFlagsProcessor.Services.Interfaces;
using BMGMoney.SDK.V2.Http;

namespace AutodecisionMultipleFlagsProcessor.Services
{
    public class CreditInquiry : ICreditInquiry
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpService _httpService;

        public CreditInquiry(IConfiguration configuration, IHttpService httpService)
        {
            _configuration = configuration;
            _httpService = httpService;
        }

        public string CheckTransunionBankruptcy(string loanNumber)
        {
            var url = _configuration["Apis:CreditInquiry"] + $"/transunion/bankruptcy?loan_number={loanNumber}";

            var result =  _httpService.GetAsync(url).Result;

            return result.Response;
        }

        public async Task GetIdVerification(int customerId)
        {
            var url = _configuration["Apis:CreditInquiry"] + $"/transunion/IDVerification?customer_id={customerId}";

            await _httpService.GetAsync(url);
        }

        public string CheckPacerBankruptcy(string loanNumber)
        {
            var url = _configuration["Apis:CreditInquiry"] + $"/pacer?loan_number={loanNumber}";

            var result = _httpService.GetAsync(url).Result;

            return result.Response;
        }
    }
}
