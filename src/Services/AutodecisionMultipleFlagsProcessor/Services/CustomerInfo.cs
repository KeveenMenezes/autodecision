using AutodecisionMultipleFlagsProcessor.DTOs;
using AutodecisionMultipleFlagsProcessor.Services.Interfaces;
using BMGMoney.SDK.V2.Http;
using Newtonsoft.Json;

namespace AutodecisionMultipleFlagsProcessor.Services
{
    public class CustomerInfo : ICustomerInfo
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpService _httpService;
        private readonly ILogger<CustomerInfo> _logger;

        public CustomerInfo(IConfiguration configuration, IHttpService httpService, ILogger<CustomerInfo> logger)
        {
            _configuration = configuration;
            _httpService = httpService;
            _logger = logger;
        }

        public async Task<IEnumerable<ApplicationDto>> GetOtherApplicationsWithSameFingerprint(string browserFingerprint, int customerId)
        {
            var url = _configuration["Apis:CustomerInfoApi"] + $"/application/get-other-applications-with-same-fingerprint?browserFingerprint={browserFingerprint}&customerId={customerId}";
            var result = await _httpService.GetAsync<IEnumerable<ApplicationDto>>(url);
            return result.Response;

        }

        public async Task<OtherOpenLoanDTO> GetOtherOpenLoan(int applicationid, int customerid, string type)
        {
            string url = _configuration["Apis:CustomerInfoApi"] + $"/application/open-loan-number?application_id={applicationid}&customer_id={customerid}&type={type}";

            var result = await _httpService.GetAsync(url);

            var response = new OtherOpenLoanDTO();

            if (result.Success)
                response = JsonConvert.DeserializeObject<OtherOpenLoanDTO>(result.Response);

            return response;
        }

        public async Task<ApplicationsWithSameBankInfoDTO> GetApplicationsWithSameBankInfo(int customer_id, string bank_routing_number, string bank_account_number)
        {
            string url = _configuration["Apis:CustomerInfoApi"] + $"/application/applications-same-bank-info?customer_id={customer_id}&bank_routing_number={bank_routing_number}&bank_account_number={bank_account_number}";

            var result = await _httpService.GetAsync(url);

            var response = new ApplicationsWithSameBankInfoDTO();

            if (result.Success && !result.Response.Contains("[]"))
                response = JsonConvert.DeserializeObject<ApplicationsWithSameBankInfoDTO>(result.Response);
            else
                response.Data = new List<ApplicationsWithSameBankInfo>();

            return response;
        }

        public async Task<bool> CheckIfRoutingNumberAlreadyExists(string routingNumber)
        {
            string url = _configuration["Apis:CustomerInfoApi"] + $"/bank-info/check-if-routing-number-already-exists?routingNumber={routingNumber}";
            var result = await _httpService.GetAsync<bool>(url);
            return result.Response;
        }

        public async Task<List<DTOs.SimilarPhoneDataDTO>> GetCustomersWithSamePhone(int customerId, string PhoneNumber, string SecondaryPhoneNumber, string WorkPhoneNumber)
        {
            string url = _configuration["Apis:CustomerInfoApi"] + $"/customer/customers-with-same-phone?customerId={customerId}&phoneNumber={PhoneNumber}&secondaryPhoneNumber={SecondaryPhoneNumber}&workPhoneNumber={WorkPhoneNumber}";
            var result = await _httpService.GetAsync<List<SimilarPhoneDataDTO>>(url);
            return result.Response;
        }

        public async Task<bool> GetDailyReceivings(int customerId)
        {
            string url = _configuration["Apis:CustomerInfoApi"] + $"/plaid/transaction-daily-receivings?customerId={customerId}";
            var result = await _httpService.GetAsync<bool>(url);
            return result.Response;
        }

        public async Task<List<SimilarCustomerDto>> GetSimilarCustomers(int customerId)
        {
            string url = _configuration["Apis:CustomerInfoApi"] + $"/customer/similar-customers?customerId={customerId}";

            var result = await _httpService.GetAsync(url);

            var response = new List<SimilarCustomerDto>();

            if (result.Success && !result.Response.Contains("[]"))
                response = JsonConvert.DeserializeObject<List<SimilarCustomerDto>>(result.Response);

            return response;
        }

        public async Task<bool> CheckFirstnetCredit(int customerId, int daysCheck)
        {
            string url = _configuration["Apis:CustomerInfoApi"] + $"/firstnet/check-firstnet-credit-history?customerId={customerId}&daysCheck={daysCheck}";
            var result = await _httpService.GetAsync<bool>(url);
            return result.Response;
        }

        public PacerValidationDto ValidateBankruptcy(int applicationId)
        {
            var url = _configuration["Apis:CustomerInfoApi"] + $"/pacer/validate-bankruptcy?applicationId={applicationId}";
            var result = _httpService.GetAsync<PacerValidationDto>(url).Result;
            return result.Response;
        }

        public ApplicationDto GetPreviousApplication(int applicationId)
        {
            var url = _configuration["Apis:CustomerInfoApi"] + $"/application/previous-application?applicationId={applicationId}";
            var result = _httpService.GetAsync<ApplicationDto>(url).Result;
            return result.Response;
        }

        public async Task<GiactResultDto> GetLastGiactResult(string loanNumber)
        {
            var url = _configuration["Apis:CustomerInfoApi"] + $"/application/giact?loanNumber={loanNumber}";
            var result = await _httpService.GetAsync<GiactResultDto>(url);
            return result.Response;
        }

        public async Task<CensusDTO> GetCensusByCustomerIdWithCriteria(int employerId, int customerId, string criteria)
        {
            string url = _configuration["Apis:CustomerInfoApi"] + $"/census/census-by-employer-id-with-criteria?employerId={employerId}&customerId={customerId}&criteria={criteria}";

            var result = await _httpService.GetAsync<CensusDTO>(url);

            if (!result.Success)
            {
                _logger.LogWarning(
                        $"CustomerId: {customerId} - couldn't get the Census information, response_code: {result.StatusCode}");
                return null;
            }

            return result.Response;
        }

        public async Task<bool> CheckHasBook(List<SimilarAddressDto> similar_address_list)
        {
            string parameters = "";

            var url = _configuration["Apis:CustomerInfoApi"] + $"/application/check-has-book?";

            for (int i = 0; i < similar_address_list.Count; i++)
            {
                if (i == 0)
                {
                    parameters = $"loanNumber={similar_address_list[i].LoanNumber}";
                }
                else
                {
                    parameters += $"&loanNumber={similar_address_list[i].LoanNumber}";
                }
            }

            var result = await _httpService.GetAsync<bool>(url + parameters);
            return result.Response;
        }

        public async Task<bool> IsWhitelistRelated(int customerId, int customerIdRelated)
        {
            var url = _configuration["Apis:CustomerInfoApi"] + $"/white-list/is-whitelist-related?customerId={customerId}&customerIdRelated={customerIdRelated}";
            var result = await _httpService.GetAsync<bool>(url);
            return result.Response;
        }

        public async Task<List<SimilarAddressDto>> CheckCustomersWithSameAddressAsync(SimilarAddressDto request)
        {
            string url = _configuration["Apis:CustomerInfoApi"] + $"/customer/customers-with-same-address";
            var result = await _httpService.PostAsync<List<SimilarAddressDto>>(url, request);
            return result.Response;
        }

        public async Task<List<SimilarAddressDto>> GetSimilarAddressListForCustomerAsync(int customerId, int levenshteinDistance)
        {
            var url = _configuration["Apis:CustomerInfoApi"] + $"/customer/similar-address-by-customer?customerId={customerId}&levenshteinDistance={levenshteinDistance}";
            var result = await _httpService.GetAsync<List<SimilarAddressDto>>(url);
            return result.Response;
        }
    }
}