using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionCore.Data.HttpRepositories.Interfaces;
using AutoMapper;
using BMGMoney.SDK.V2.Http;

namespace AutodecisionCore.Data.HttpRepositories
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpService _httpService;
        private readonly ILogger<CustomerRepository> _logger;

        public CustomerRepository(IConfiguration configuration, IHttpService httpService, ILogger<CustomerRepository> logger)
        {
            _configuration = configuration;
            _httpService = httpService;
            _logger = logger;
        }

        public async Task<Customer> GetCustomerInfo(int customerId)
        {
            try
            {
                var url = _configuration["Apis:CustomerInfoApi"] + $"/customer/autodecision-info/{customerId}";

                var result = await _httpService.GetAsync<Customer>(url);
                if (!result.Success)
                {
                    _logger.LogWarning($"CustomerId: {customerId} - couldn't get the customer information, response_code: {result.StatusCode}");
                    return new Customer();
                }

                return result.Response;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error when trying to get customer information for CustomerId: {customerId} | Error: {ex.Message}");
                throw;
            }
        }
    }
}
