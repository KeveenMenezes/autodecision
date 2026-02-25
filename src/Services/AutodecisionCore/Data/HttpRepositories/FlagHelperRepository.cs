using AutodecisionCore.Contracts.ViewModels.Helpers;
using AutodecisionCore.Data.HttpRepositories.DTOs;
using AutodecisionCore.Data.HttpRepositories.Interfaces;
using AutoMapper;
using BMGMoney.SDK.V2.Http;

namespace AutodecisionCore.Data.HttpRepositories
{
    public class FlagHelperRepository : IFlagHelperRepository
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpService _httpService;
        private readonly ILogger<CensusRepository> _logger;

        public FlagHelperRepository(IConfiguration configuration, IHttpService httpService, ILogger<CensusRepository> logger)
        {
            _configuration = configuration;
            _httpService = httpService;
            _logger = logger;
        }

        public async Task<FlagValidatorHelper> GetFlagHelperInformationAsync(int customerId, int employerId, int applicationId)
        {
            try
            {
                var url = _configuration["Apis:CustomerInfoApi"] + $"/flag-helper/info?customerId={customerId}&employerId={employerId}&applicationId={applicationId}";

                var result = await _httpService.GetAsync<FlagHelperValidatorDTO>(url);
                if (!result.Success)
                {
                    _logger.LogWarning($"Customer Id: {customerId} and Employer Id: {employerId} - couldn't get the Flag Helper Information, response_code: {result.StatusCode}");
                    return new FlagValidatorHelper();
                }

                var config = new MapperConfiguration(c =>
                c.CreateMap<FlagHelperValidatorDTO, FlagValidatorHelper>()
                        .ConvertUsing(src => MapFlagHelperValidatorDto(src)));

                var mapper = new Mapper(config);

                return mapper.Map<FlagValidatorHelper>(result.Response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error when trying to get Flag Helper information for Customer Id: {customerId} and Employer Id: {employerId} | Error: {ex.Message}");
                throw;
            }
        }

        private static FlagValidatorHelper? MapFlagHelperValidatorDto(FlagHelperValidatorDTO src)
        {
            if (src == null) return null;

            return new FlagValidatorHelper()
            {
                EmployerAllowAutoDeny = src.EmployerAllowAutoDeny,
                CustomerSkipAutoDeny = CustomerSkipAutoDenyDTO.MapCustomerSkipAutoDeny(src.CustomerSkipAutoDeny),
                ApplicationDocuments = ApplicationDocumentDTO.MapApplicationDocuments(src.ApplicationDocuments)
            };
        }
    }
}