using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionCore.Data.HttpRepositories.DTOs;
using AutodecisionCore.Data.HttpRepositories.Interfaces;
using AutoMapper;
using BMGMoney.SDK.V2.Http;

namespace AutodecisionCore.Data.HttpRepositories
{
    public class CensusRepository : ICensusRepository
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpService _httpService;
        private readonly ILogger<CensusRepository> _logger;

        public CensusRepository(IConfiguration configuration, IHttpService httpService, ILogger<CensusRepository> logger)
        {
            _configuration = configuration;
            _httpService = httpService;
            _logger = logger;
        }

        public async Task<Census> GetCensusDataByCustomerId(int customerId, int employerId)
        {
            try
            {
                var url = _configuration["Apis:CustomerInfoApi"] + $"/census/autodecision-info?customerId={customerId}&employerId={employerId}";

                var result = await _httpService.GetAsync<CensusDTO>(url);
                if (!result.Success)
                {
                    _logger.LogWarning($"CustomerId: {customerId} - couldn't get the Census Information, response_code: {result.StatusCode}");
                    return new Census();
                }

                var config = new MapperConfiguration(c => c.CreateMap<CensusDTO, Census>());
                var mapper = new Mapper(config);

                return mapper.Map<Census>(result.Response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error when trying to get Census information for CustomerId: {customerId} | Error: {ex.Message}");
                throw;
            }
        }
    }
}
