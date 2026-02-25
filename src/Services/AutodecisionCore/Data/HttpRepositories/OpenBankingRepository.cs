using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionCore.Data.HttpRepositories.Interfaces;
using BMGMoney.SDK.V2.Http;

namespace AutodecisionCore.Data.HttpRepositories
{
    public class OpenBankingRepository : IOpenBankingRepository
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpService _httpService;
        private readonly ILogger<OpenBankingRepository> _logger;

        public OpenBankingRepository(IConfiguration configuration, IHttpService httpService, ILogger<OpenBankingRepository> logger)
        {
            _configuration = configuration;
            _httpService = httpService;
            _logger = logger;
        }

        public async Task<OpenBanking> GetOpenBankingInfo(int customerId)
        {
            try
            {
                var url = _configuration["Apis:CustomerInfoApi"] + $"/plaid/autodecision-info/{customerId}";

                var result = await _httpService.GetAsync<OpenBanking>(url);
                if (!result.Success)
                {
                    _logger.LogWarning($"CustomerId: {customerId} - couldn't get the open banking information, response_code: {result.StatusCode}");
                    return new OpenBanking();
                }

                return result.Response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error when trying to get open banking information for CustomerId: {customerId} | Error: {ex.Message}");
                throw;
            }
        }
    }
}
