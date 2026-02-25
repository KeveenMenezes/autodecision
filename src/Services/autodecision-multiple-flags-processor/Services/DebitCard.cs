using AutodecisionMultipleFlagsProcessor.DTOs;
using AutodecisionMultipleFlagsProcessor.Handlers;
using AutodecisionMultipleFlagsProcessor.Services.Interfaces;
using BMGMoney.SDK.V2.Http;

namespace AutodecisionMultipleFlagsProcessor.Services
{
    public class DebitCard : IDebitCard
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpService _httpService;
		private readonly ILogger<DebitCard> _logger;

		public DebitCard(IConfiguration configuration, IHttpService httpService, ILogger<DebitCard> logger)
        {
            _configuration = configuration;
            _httpService = httpService;
            _logger = logger;
        }

        public async Task<AccountsCustomerDTO> GetAccountsCustomer(int customerId)
        {
            var url = _configuration["Apis:DebitCardApi"] + $"/accounts?customerId={customerId}";
            _logger.LogInformation("--- URL to call DebitCardApi: " + url + " ---");

            var result = await _httpService.GetAsync<AppServiceBaseResponse<AccountsCustomerDTO>>(url);

            return result.Response.Data;

        }
    }
}
