using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionCore.Data.HttpRepositories.DTOs;
using AutodecisionCore.Data.HttpRepositories.Interfaces;
using AutoMapper;
using BMGMoney.SDK.V2.Http;

namespace AutodecisionCore.Data.HttpRepositories
{
    public class ApplicationWarningRepository : IApplicationWarningRepository
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpService _httpService;
        private readonly ILogger<ApplicationWarningRepository> _logger;

        public ApplicationWarningRepository(
            IConfiguration configuration,
            IHttpService httpService,
            ILogger<ApplicationWarningRepository> logger)
        {
            _configuration = configuration;
            _httpService = httpService;
            _logger = logger;
        }

        public async Task<List<ApplicationWarning>> GetApplicationWarningsInfo(int applicationId)
        {
            try
            {
                var url = _configuration["Apis:CustomerInfoApi"] + $"/application-warning/autodecision-info/{applicationId}";

                var result = await _httpService.GetAsync<List<ApplicationWarningDTO>>(url);
                if (!result.Success)
                {
                    _logger.LogWarning($"ApplicationId: {applicationId} - couldn't get the Application Warning information, response_code: {result.StatusCode}");
                    return new List<ApplicationWarning>();
                }
                var config = new MapperConfiguration(c => c.CreateMap<ApplicationWarningDTO, ApplicationWarning>());
                var mapper = new Mapper(config);

                return mapper.Map<List<ApplicationWarning>>(result.Response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error while trying to get Application Warning Info for ApplicationId: {applicationId} | Error: {ex.Message}");
                throw;
            }
        }
    }
}
