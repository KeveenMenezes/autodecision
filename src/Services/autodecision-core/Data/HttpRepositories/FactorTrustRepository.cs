using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionCore.Data.HttpRepositories.Interfaces;
using BMGMoney.SDK.V2.Http;
using static System.Net.Mime.MediaTypeNames;

namespace AutodecisionCore.Data.HttpRepositories
{
    public class FactorTrustRepository : IFactorTrustRepository
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpService _httpService;
        private readonly ILogger<FactorTrustRepository> _logger;

        public FactorTrustRepository(IConfiguration configuration, IHttpService httpService, ILogger<FactorTrustRepository> logger)
        {
            _configuration = configuration;
            _httpService = httpService;
            _logger = logger;
        }

        public async Task<FactorTrust?> GetFactorTrustInfo(int customerId, string loanNumber) 
        {
            try
            {
                var url = _configuration["Apis:CustomerInfoApi"] + $"/factor-trust/autodecision-info?customerId={customerId}&loanNumber={loanNumber}"; 

                var result = await _httpService.GetAsync<FactorTrust>(url);
                if (!result.Success)
                {
                    _logger.LogWarning($"LoanNumber: {loanNumber} and CustomerId: {customerId} - couldn't get factor trust information, response_code: {result.StatusCode}");
                    return null;
                }

                return result.Response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error when trying to get factor trust information for loanNumber: {loanNumber} | Error: {ex.Message}");
                throw;
            }
        }
    }
}
