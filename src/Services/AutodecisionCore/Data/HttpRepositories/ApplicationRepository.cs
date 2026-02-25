using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionCore.Data.HttpRepositories.DTOs;
using AutodecisionCore.Data.HttpRepositories.Interfaces;
using AutodecisionCore.DTOs;
using AutodecisionCore.Events;
using AutodecisionCore.Utils;
using AutoMapper;
using BMGMoney.SDK.V2.Http;
using Newtonsoft.Json;

namespace AutodecisionCore.Data.HttpRepositories
{
    public class ApplicationRepository : IApplicationRepository
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpService _httpService;
        private readonly ILogger<ApplicationRepository> _logger;
        private readonly string _applicationCoreUrl;

        public ApplicationRepository(
            IConfiguration configuration,
            IHttpService httpService,
            ILogger<ApplicationRepository> logger)
        {
            _configuration = configuration;
            _httpService = httpService;
            _logger = logger;
            _applicationCoreUrl = _configuration["Apis:ApiUrlCoreApplication"] + "/api/v1/";
        }

        public async Task<Application?> GetApplicationInfo(string loanNumber)
        {
            try
            {
                var url = _configuration["Apis:CustomerInfoApi"] + $"/application/autodecision-info/{loanNumber}";

                var result = await _httpService.GetAsync<ApplicationDTO>(url);
                if (!result.Success)
                {
                    _logger.LogWarning(
                        $"LoanNumber: {loanNumber} - couldn't get the Application information, response_code: {result.StatusCode}");
                    return null;
                }

                var config = new MapperConfiguration(c =>
                {
                    c.CreateMap<ApplicationDTO, Application>()
                        .ConvertUsing(src => MapApplicationDto(src));
                });

                var mapper = new Mapper(config);

                return mapper.Map<Application>(result.Response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error when trying to get Application information for loanNumber: {loanNumber} | Error: {ex.Message}");
                throw;
            }
        }

        public async Task<Application?> GetApplicationInfoByCustomerId(int customerId)
        {
            try
            {
                var url = _configuration["Apis:CustomerInfoApi"] + $"/application/autodecision-info/customer/{customerId}";

                var result = await _httpService.GetAsync<ApplicationDTO>(url);
                if (!result.Success)
                {
                    _logger.LogWarning(
                        $"Customer Id: {customerId} - couldn't get the Application information, response_code: {result.StatusCode}");
                    return null;
                }

                var config = new MapperConfiguration(c =>
                {
                    c.CreateMap<ApplicationDTO, Application>()
                        .ConvertUsing(src => MapApplicationDto(src));
                });

                var mapper = new Mapper(config);

                return mapper.Map<Application>(result.Response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error when trying to get Application information.| Error: {ex.Message}");
                throw;
            }
        }

        public async Task<List<LastApplication>> GetLastApplications(int customerId, string loanNumber)
        {
            try
            {
                var url = _configuration["Apis:CustomerInfoApi"] +
                          $"/application/last-applications?customerId={customerId}&loanNumber={loanNumber}";

                var result = await _httpService.GetAsync<List<LastApplicationDTO>>(url);
                if (!result.Success)
                {
                    _logger.LogWarning(
                        $"LoanNumber: {loanNumber} - couldn't get the Application information, response_code: {result.StatusCode}");
                    return new List<LastApplication>();
                }

                var config = new MapperConfiguration(c =>
                {
                    c.CreateMap<LastApplicationDTO, LastApplication>()
                        .ConvertUsing(src => MapLastApplicationDto(src));
                });

                var mapper = new Mapper(config);

                return mapper.Map<List<LastApplication>>(result.Response);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error when trying to get last Applications for loanNumber: {loanNumber} | Error: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> ApproveApplication(ApproveApplicationRequestEvent request)
        {
            try
            {
                _logger.LogInformation($"Approving Application for LoanNumber: {request.LoanNumber}");

                var url = _configuration["Apis:CustomerInfoApi"] + $"/application/autodecision-core/approve-application";

                var result = await _httpService.PostAsync<bool>(url, request);
                if (!result.Success)
                {
                    _logger.LogWarning(
                        $"LoanNumber: {request.LoanNumber} - couldn't approve Application, response_code: {result.StatusCode}");
                    return false;
                }
                return result.Response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error when trying to approve Application for loanNumber: {request.LoanNumber} | Error: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> DeclineApplication(DeclineApplicationRequestEvent request)
        {
            try
            {
                var url = _configuration["Apis:CustomerInfoApi"] +
                          $"/application/autodecision-core/decline-application";

                var result = await _httpService.PostAsync<bool>(url, request);
                if (!result.Success)
                {
                    _logger.LogWarning(
                        $"LoanNumber: {request.LoanNumber} - couldn't decline Application, response_code: {result.StatusCode}");
                    return false;
                }

                return result.Response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error when trying to decline Application for loanNumber: {request.LoanNumber} | Error: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> RequireDocuments(RequireDocumentsRequestEvent request)
        {
            try
            {
                var url = _configuration["Apis:CustomerInfoApi"] + $"/application/autodecision-core/require-documents";

                var result = await _httpService.PostAsync<bool>(url, request);
                if (!result.Success)
                {
                    _logger.LogWarning(
                        $"LoanNumber: {request.LoanNumber} - couldn't require documents, response_code: {result.StatusCode}");
                    return false;
                }
                return result.Response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error when trying to require documents for loanNumber: {request.LoanNumber} | Error: {ex.Message}");
                throw;
            }
        }

        public async Task NotifyDefaultDocuments(NotifyDefaultDocumentsRequestEvent request)
        {
            try
            {
                var url = _configuration["Apis:CustomerInfoApi"] + $"/application/autodecision-core/default-documents";

                var result = await _httpService.PostAsync<bool>(url, request);
                if (!result.Success)
                {
                    _logger.LogWarning(
                        $"LoanNumber: {request.LoanNumber} - couldn't notify default documents, response_code: {result.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error when trying to notify default documents for loanNumber: {request.LoanNumber} | Error: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> RequireAllotment(RequireAllotmentRequestEvent request)
        {
            try
            {
                var url = _configuration["Apis:CustomerInfoApi"] + $"/application/autodecision-core/require-allotment";

                var result = await _httpService.PostAsync<bool>(url, request);
                if (!result.Success)
                {
                    _logger.LogWarning(
                        $"LoanNumber: {request.LoanNumber} - couldn't require allotment, response_code: {result.StatusCode}");
                    return false;
                }
                return result.Response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error when trying to require allotment for loanNumber: {request.LoanNumber} | Error: {ex.Message}");
                throw;
            }
        }


        public async Task<TotalIncome> GetTotalIncomeDetailsByApplicationIdAsync(int applicationId)
        {
            try
            {
                var url = _applicationCoreUrl + $"income/total-income/{applicationId}/details";
                Dictionary<string, string> headers = Token.GetHeaders();

                HttpClient client = new HttpClient();
                HttpResponseMessage response = await client.SendAsync(CreateHttpRequestWithHeaders(url, headers));
                client.DefaultRequestHeaders.Clear();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning($"Application: {applicationId} - couldn't get the Application Income information, response_code: {response.StatusCode}");
                    return new TotalIncome();
                }

                var result = JsonConvert.DeserializeObject<ApiResultDto<TotalIncome>>(await response.Content.ReadAsStringAsync());
                if (result is null)
                    return new TotalIncome();

                return result.Data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error while trying to get Customer Income Info. | Application: {applicationId} | Error: {ex.Message}");
                throw;
            }
        }



        private static Application? MapApplicationDto(ApplicationDTO src)
        {
            if (src == null) return null;
            if (src.Id == 0 && string.IsNullOrEmpty(src.LoanNumber)) return null;

            return new Application
            {
                Id = src.Id,
                LoanNumber = src.LoanNumber,
                Type = src.Type,
                AmountOfPayment = src.AmountOfPayment,
                SubmittedAt = src.SubmittedAt ?? DateTime.MinValue,
                BrowserFingerprint = src.BrowserFingerprint,
                LoanAmount = src.LoanAmount,
                EmployerId = src.EmployerId,
                EmployerName = src.EmployerName,
                EmployerKey = src.EmployerKey,
                EmployerPaymentType = src.EmployerPaymentType,
                EmploymentLengthRangeId = src.EmploymentLengthRangeId ?? 0,
                FundingMethod = src.FundingMethod,
                ProductId = src.ProductId ?? 0,
                ProductKey = src.ProductKey,
                BankRoutingNumber = src.BankRoutingNumber,
                BankAccountNumber = src.BankAccountNumber,
                Program = src.Program,
                PhoneNumber = src.PhoneNumber,
                Status = src.Status,
                StateAbbreviation = src.StateAbbreviation,
                LoanTermsStateAbbreviation = src.LoanTermsStateAbbreviation,
                StateIpUserRequest = src.StateIpUserRequest,
                CustomerId = src.CustomerId,
                PaymentType = src.PaymentType,
                PreviousApplicationId = src.PreviousApplicationId ?? 0,
                IsEmployerCensusEligible = src.IsEmployerCensusEligible,
                UwCluster = src.UwCluster,
                ReconciliationSystem = src.ReconciliationSystem,
                HighRisk = src.HighRisk,
                CreatedBy = src.CreatedBy,
                TurndownActive = src.TurndownActive,
                AllotmentAccountNumber = src.AllotmentAccountNumber,
                AllotmentRoutingNumber = src.AllotmentRoutingNumber,
                VerifiedNetIncome = src.VerifiedNetIncome,
                IsWebBankRollout = src.IsWebBankRollout,
                VerifiedDateOfHire = src.VerifiedDateOfHire,
                PartnerId = src.PartnerId,
                EmployerPartnerId = src.EmployerPartnerId,
                EmployerIsAssociation = src.EmployerIsAssociation,
                PartnerName = src.PartnerName,
                ProductIdCreditPolicy = src.ProductIdCreditPolicy,
                JpmExistsPending = src.JpmExistsPending,
                IsProbableCashApp = src.IsProbableCashApp
            };
        }

        private static LastApplication? MapLastApplicationDto(LastApplicationDTO src)
        {
            if (src == null) return null;
            if (src.Id == 0 && string.IsNullOrEmpty(src.LoanNumber)) return null;

            return new LastApplication
            {
                Id = src.Id,
                LoanNumber = src.LoanNumber,
                Type = src.Type,
                AmountOfPayment = src.AmountOfPayment,
                BrowserFingerprint = src.BrowserFingerprint,
                BankRoutingNumber = src.BankRoutingNumber,
                BankAccountNumber = src.BankAccountNumber,
                Status = src.Status,
                CreatedAt = src.CreatedAt,
                ReconciliationSystem = src.ReconciliationSystem,
                ApplicationConnections = MapApplicationConnectionsDTO(src.ApplicationConnections)
            };
        }

        private static ApplicationConnections? MapApplicationConnectionsDTO(ApplicationConnectionsDTO src)
        {
            if (src == null) return null;

            return new ApplicationConnections
            {
                ApplicationId = src.application_id,
                OpenBankingId = src.open_banking_id,
                OpenPayrollId = src.open_payroll_id,
                FaceId = src.face_id,
                DebitCardId = src.debit_card_id
            };
        }

        private static HttpRequestMessage CreateHttpRequestWithHeaders(string url, Dictionary<string, string>? tokenHeaders = null)
        {
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(url),
            };

            if (tokenHeaders != null)
            {
                foreach (var header in tokenHeaders)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
            }

            return request;
        }
    }
}