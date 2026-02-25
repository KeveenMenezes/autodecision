using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionCore.Data.HttpRepositories.Interfaces;
using AutodecisionCore.DTOs;
using AutodecisionCore.Services.Interfaces;
using BMGMoney.SDK.V2.Http;

namespace AutodecisionCore.Services.HttpServices
{
    public class CreditInquiryService : ICreditInquiryService
    {
        private readonly ILogger<CreditInquiryService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IHttpService _httpService;
        private readonly ITransunionRepository _transunionRepository;

        public CreditInquiryService(
            ILogger<CreditInquiryService> logger,
            IConfiguration configuration,
            IHttpService httpService, 
            ITransunionRepository transunionRepository)
        {
            _logger = logger;
            _configuration = configuration;
            _httpService = httpService;
            _transunionRepository = transunionRepository;
        }

        public async Task RunClarityInquiry(Application application)
        {
            try
            {
                _logger.LogInformation($"Clarity Identity Priority Loan: {application.LoanNumber} - Starting Customer Identity Process");

                var url = _configuration["Apis:CreditInquiryApi"] + "/clarity?loan_number=" + application.LoanNumber;
                var result = await _httpService.GetAsync<ResultDTO>(url);

                if (!result.Success || !result.Response.Success)
                    _logger.LogWarning($"LoanNumber: {application.LoanNumber} - couldn't run the Clarity Identity request | Response - code: {result.StatusCode} - message: {result.Response.Message}");

                _logger.LogInformation($"Request to Run the Clarity Identity Finished: {application.LoanNumber}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error when trying to execute Clarity Identity Request for LoanNumber: {application.LoanNumber} | Error: {ex.Message}");
            }
        }

        public async Task RunGiact(Application application)
        {
            try
            {
                _logger.LogInformation($"Request to Run Giact Started: {application.LoanNumber}");

                var url = _configuration["Apis:CreditInquiryApi"] + "/giact?loan_number=" + application.LoanNumber;
                var result = await _httpService.GetAsync<ResultDTO>(url);

                if (!result.Success || !result.Response.Success)
                    _logger.LogWarning($"LoanNumber: {application.LoanNumber} - couldn't run the Giact request| Response - code: {result.StatusCode} - message: {result.Response.Message}");

                _logger.LogInformation($"Request to Run the Giact Finished: {application.LoanNumber}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error when trying to execute Giact Request for LoanNumber: {application.LoanNumber} | Error: {ex.Message}");
            }
        }

        public async Task RunFedsDataCenter(Application application)
        {
            try
            {
                _logger.LogInformation($"Request to Run Feds Data Center Started: {application.LoanNumber}");

                var url = _configuration["Apis:CreditInquiryApi"] + "/fedsdatacenter?loan_number=" + application.LoanNumber;
                var result = await _httpService.GetAsync<ResultDTO>(url);

                if (!result.Success || !result.Response.Success)
                    _logger.LogWarning($"LoanNumber: {application.LoanNumber} - couldn't run the Feds Data Center request| Response - code: {result.StatusCode} - message: {result.Response.Message}");

                _logger.LogInformation($"Request to Run the Feds Data Center Finished: {application.LoanNumber}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error when trying to execute Feds Data Center Request for LoanNumber: {application.LoanNumber} | Error: {ex.Message}");
            }
        }

        public async Task RunEquifaxWorkNumberInquiry(Application application)
        {
            try
            {
                _logger.LogInformation($"Request to Run Equifax WorkNumber Inquiry Started: {application.LoanNumber}");

                var url = _configuration["Apis:CreditInquiryApi"] + "/EquifaxWorkNumber?loan_number=" + application.LoanNumber;
                var result = await _httpService.GetAsync<ResultDTO>(url);

                if (!result.Success || !result.Response.Success)
                    _logger.LogWarning($"LoanNumber: {application.LoanNumber} - couldn't run the Equifax WorkNumber Inquiry request| Response - code: {result.StatusCode} - message: {result.Response.Message}");

                _logger.LogInformation($"Request to Run Equifax WorkNumber Inquiry Finished: {application.LoanNumber}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error when trying to execute Equifax WorkNumber Inquiry Request for LoanNumber: {application.LoanNumber} | Error: {ex.Message}");
            }
        }

        public async Task RunFactorTrustInquiry(Application application) 
        {
            try
            {
                _logger.LogInformation($"Request to Run Factor Trust Inquiry Started: {application.LoanNumber}");

                var url = _configuration["Apis:CreditInquiryApi"] + "/factortrust?loan_number=" + application.LoanNumber;
                var result = await _httpService.GetAsync<ResultDTO>(url);

                if (!result.Success || !result.Response.Success)
                    _logger.LogWarning($"LoanNumber: {application.LoanNumber} - couldn't run the Factor Trust Inquiry request| Response - code: {result.StatusCode} - message: {result.Response.Message}");

                _logger.LogInformation($"Request to Run Factor Trust Inquiry Finished: {application.LoanNumber}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error when trying to execute Trust Inquiry Request for LoanNumber: {application.LoanNumber} | Error: {ex.Message}");
            }
        }

        public async Task StartTransunionProcess(Application application)
        {
            try
            {
                if (await _transunionRepository.IsTransunionProcessedForCustomer(application.CustomerId))
                    return;

                _logger.LogInformation($"Request to Start Transunion Process Started: {application.LoanNumber}");

                var url = _configuration["Apis:CreditInquiryApi"] + "/transunion/IDVerification?customer_id=" + application.CustomerId;
                var result = await _httpService.GetAsync<ResultDTO>(url);

                if (!result.Success || !result.Response.Success)
                    _logger.LogWarning($"LoanNumber: {application.LoanNumber} - couldn't run the Start for Transunion Process request| Response - code: {result.StatusCode} - message: {result.Response.Message}");

                _logger.LogInformation($"Request to Start Transunion Process Finished: {application.LoanNumber}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error when trying to execute the Start Transunion Process Request for LoanNumber: {application.LoanNumber} | Error: {ex.Message}");
                throw;
            }
        }

    }
}
