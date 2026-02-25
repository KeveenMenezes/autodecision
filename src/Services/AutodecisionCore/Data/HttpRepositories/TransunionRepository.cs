using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionCore.Data.HttpRepositories.DTOs;
using AutodecisionCore.Data.HttpRepositories.Interfaces;
using AutodecisionCore.DTOs;
using AutoMapper;
using BMGMoney.SDK.V2.Http;

namespace AutodecisionCore.Data.HttpRepositories
{
    public class TransunionRepository : ITransunionRepository
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpService _httpService;
        private readonly ILogger<TransunionRepository> _logger;

        public TransunionRepository(
            IConfiguration configuration,
            IHttpService httpService,
            ILogger<TransunionRepository> logger)
        {
            _configuration = configuration;
            _httpService = httpService;
            _logger = logger;
        }

        public async Task<TransunionResult> GetTransunionResultInfo(int customerId)
        {
            try
            {
                var url = _configuration["Apis:CustomerInfoApi"] + $"/transunion/autodecision-info/{customerId}";

                var result = await _httpService.GetAsync<TransunionResultDTO>(url);
                if (!result.Success)
                {
                    _logger.LogWarning($"CustomerId: {customerId} - couldn't get the Transunion Result, response_code: {result.StatusCode}");
                    return new TransunionResult();
                }

                var config = new MapperConfiguration(c => c.CreateMap<TransunionResultDTO, TransunionResult>());
                var mapper = new Mapper(config);

                return mapper.Map<TransunionResult>(result.Response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error when trying to get Transunion Result. | Error: {ex.Message}");
                throw;
            }
        }


        public async Task<TransunionResult> GetClarityTransunionResultInfo(int customerId)
        {
            try
            {
                var url = _configuration["Apis:CustomerInfoApi"] + $"/transunion/autodecision-info-clarity/{customerId}";

                var result = await _httpService.GetAsync<TransunionResultDTO>(url);
                if (!result.Success)
                {
                    _logger.LogWarning($"CustomerId: {customerId} - couldn't get the Clarity Data From Transunion Result, response_code: {result.StatusCode}");
                    return new TransunionResult();
                }

                var config = new MapperConfiguration(c => c.CreateMap<TransunionResultDTO, TransunionResult>());
                var mapper = new Mapper(config);

                return mapper.Map<TransunionResult>(result.Response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error when trying to get Clarity Transunion Result. | Error: {ex.Message}");
                throw;
            }
        }

        public async Task DeactivateOldRecords(int customerId)
        {
            try
            {
                var url = _configuration["Apis:CustomerInfoApi"] + "/transunion/deactivate-old-records";
                var data = new RequestDeactiveTransunion {CustomerId = customerId };

                var result = await _httpService.PostAsync<ResultDTO>(url, data);
                if (!result.Success)
                    _logger.LogWarning($"CustomerId: {customerId} - couldn't get the Transunion Result, response_code: {result.StatusCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error while trying to Deactivate Old Transunion Records. | Error: {ex.Message}");
                throw;
            }
        }

        public async Task CreateNewTransunionResultFromClarity(Application application)
        {
            try
            {
                var data = new CreateTransunionDTO { CustomerId = application.CustomerId, LoanNumber = application.LoanNumber };
                var url = _configuration["Apis:CustomerInfoApi"] + "/transunion/create";

                var result = await _httpService.PostAsync<ResultDTO>(url, data);
                if (!result.Success)
                    _logger.LogWarning($"CustomerId: {application.CustomerId} - couldn't get the Transunion Result, response_code: {result.StatusCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error while trying to Create a New Transunion Result From Clarity. | Error: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> IsTransunionProcessedForCustomer(int customerId)
        {
            try
            {
                var url = _configuration["Apis:CustomerInfoApi"] + $"/transunion/already-processed/{customerId}";
                var result = await _httpService.GetAsync<bool>(url);

                if (result.Success) 
                    return result.Response;

                _logger.LogWarning($"CustomerId: {customerId} - couldn't get the Transunion Result, response_code: {result.StatusCode}");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error while trying to verify if Transunion is already processed. | Error: {ex.Message}");
                throw;
            }
        }

    }
}
