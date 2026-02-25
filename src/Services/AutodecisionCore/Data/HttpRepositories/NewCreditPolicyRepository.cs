using AutodecisionCore.Contracts.Constants;
using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionCore.Data.HttpRepositories.DTOs;
using AutodecisionCore.Data.HttpRepositories.Interfaces;
using AutodecisionCore.DTOs;
using AutodecisionCore.Extensions;
using AutodecisionCore.Utils;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace AutodecisionCore.Data.HttpRepositories;

public class NewCreditPolicyRepository : INewCreditPolicyRepository
{
    private readonly ILogger<NewCreditPolicyRepository> _logger;
    private readonly string _baseUrl;

    public NewCreditPolicyRepository(ILogger<NewCreditPolicyRepository> logger)
    {
        _logger = logger;
        _baseUrl = Environment.GetEnvironmentVariable("API_URL_CREDIT_POLICY") + "/api/v1/";
    }
    public async Task<CreditPolicy> GetNewCreditPolicyAndRules(Application application)
    {
        try
        {
            if (!application.ProductIdCreditPolicy.HasValue)
                application.ProductIdCreditPolicy = 1;

            Dictionary<string, string> headers = Token.GetHeaders();

            CreditPolicyEntity? creditPolicyResult = await FetchCreditPolicy(application, headers);
            if (creditPolicyResult == null)
                return SetDefaultCreditPolicyResult();

            List<NewEmployerRulesDto> rulesResult = await FetchCreditPolicyRules(application, headers);

            return await SetNewCreditPolicyResult(rulesResult, creditPolicyResult, application.ProductId, headers);
        }
        catch (Exception ex)
        {
            string message = GetErrorMessage(application, isRulesMessage: false);
            _logger.LogError(ex, message);
            return SetDefaultCreditPolicyResult();
        }
    }

    private async Task<CreditPolicyEntity?> FetchCreditPolicy(Application application, Dictionary<string, string> headers)
    {
        HttpClient client = new HttpClient();
        HttpResponseMessage response = await client.SendAsync(CreateHttpRequestWithHeaders(headers, GetUrlToFetchCreditPolicy(application)));
        client.DefaultRequestHeaders.Clear();

        if (!response.IsSuccessStatusCode)
        {
            string message = GetErrorMessage(application, isRulesMessage: false);
            _logger.LogError(message);
            return null;
        }

        var getCreditPolicyResult = JsonConvert.DeserializeObject<ApiResultDto<List<CreditPolicyEntity>>>(await response.Content.ReadAsStringAsync());
        if (getCreditPolicyResult == null || getCreditPolicyResult.Data.Count == 0)
            return null;

        return getCreditPolicyResult.Data.FirstOrDefault();
    }

    private async Task<List<NewEmployerRulesDto>> FetchCreditPolicyRules(Application application, Dictionary<string, string> headers)
    {
        HttpClient client = new HttpClient();
        HttpResponseMessage response = await client.SendAsync(CreateHttpRequestWithHeaders(headers, GetUrlToFetchRules(application)));
        client.DefaultRequestHeaders.Clear();

        if (!response.IsSuccessStatusCode)
        {
            string message = GetErrorMessage(application, isRulesMessage: true);
            _logger.LogError(message);
            return new List<NewEmployerRulesDto>();
        }

        var rulesResult = JsonConvert.DeserializeObject<ApiResultDto<List<NewEmployerRulesDto>>>(await response.Content.ReadAsStringAsync());
        if (rulesResult == null)
            return new List<NewEmployerRulesDto>();

        return rulesResult.Data;
    }

    private async Task<ParameterCreditPolicyDTO> FetchCommitmentLevelForCashless(Dictionary<string, string> headers)
    {
        try
        {
            HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.SendAsync(CreateHttpRequestWithHeaders(headers, GetUrlToFetchLOCCashless(CreditPolicyParameterKey.CashlessCommitmentLevel)));
            client.DefaultRequestHeaders.Clear();

            if (!response.IsSuccessStatusCode)
            {
                string message = GetErrorMessageForGetLOCCashless(CreditPolicyParameterKey.CashlessCommitmentLevel);
                _logger.LogError(message);
                return new ParameterCreditPolicyDTO();
            }

            var cashlessResult = JsonConvert.DeserializeObject<ApiResultDto<ParameterCreditPolicyDTO>>(await response.Content.ReadAsStringAsync());
            if (cashlessResult == null)
                return new ParameterCreditPolicyDTO();

            return cashlessResult.Data;
        }
        catch (Exception ex)
        {
            string message = GetErrorMessageForGetLOCCashless(CreditPolicyParameterKey.CashlessCommitmentLevel);
            _logger.LogError(ex, message);
            return new ParameterCreditPolicyDTO();
        }
    }

    private static HttpRequestMessage CreateHttpRequestWithHeaders(Dictionary<string, string> tokenHeaders, string url)
    {
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri(url),
        };

        foreach (var header in tokenHeaders)
            request.Headers.Add(header.Key, header.Value);

        return request;
    }

    private async Task<CreditPolicy> SetNewCreditPolicyResult(List<NewEmployerRulesDto> rules, CreditPolicyEntity creditPolicy, int productId, Dictionary<string, string> tokenHeaders)
    {
        var result = creditPolicy.MapToCreditPolicy(rules);

        if (productId == ApplicationProductId.Cashless)
        {
            ParameterCreditPolicyDTO cashlessLoc = await FetchCommitmentLevelForCashless(tokenHeaders);
            result.CreditPolicyEntity.LocRefi = cashlessLoc.DecimalValue;
        }

        return result;
    }

    private static CreditPolicy SetDefaultCreditPolicyResult(CreditPolicyEntity? creditPolicy = null) =>
         new()
         {
             CreditPolicyEntity = creditPolicy ?? new CreditPolicyEntity(),
             EmployerRules = new EmployerRules()
             {
                 EmployerRulesItems = new List<EmployerRulesItem>()
             }
         };

    private string GetUrlToFetchRules(Application application)
    {
        string url = _baseUrl + $"rules/application?StateAbbreviation={application.StateAbbreviation}&ApplicationType={application.Type}&EmployerId={application.EmployerId}&ProductId={application.ProductIdCreditPolicy}&LoanAmount={application.LoanAmount}";
        return url.Replace(",", ".");
    }
    private string GetUrlToFetchCreditPolicy(Application application)
    {
        string url = _baseUrl + $"credit-policy?StateAbbreviation={application.StateAbbreviation}&ApplicationType={application.Type}&EmployerId={application.EmployerId}&ProductId={application.ProductIdCreditPolicy}&LoanAmount={application.LoanAmount}";
        return url.Replace(",", ".");
    }

    private string GetUrlToFetchLOCCashless(string parameterKey)
    {
        string url = _baseUrl + $"parameters/key?ParameterKey={parameterKey}";
        return url.Replace(",", ".");
    }

    private static string GetErrorMessage(Application application, bool isRulesMessage)
    {
        string baseApi = isRulesMessage ? "Employer Rules" : "Credit Policy";
        return @$"Error when trying to get {baseApi} with filters: StateAbbreviation - {application.StateAbbreviation}, 
                ApplicationType - {application.Type}, EmployerId - {application.EmployerId}, 
                ProductId - {application.ProductId}, ProductIdCreditPolicy - {application.ProductIdCreditPolicy}, Loan Amount - {application.LoanAmount}";
    }

    private static string GetErrorMessageForGetLOCCashless(string parameterKey) =>
        @$"Error when trying to get LOC Cashless with filter: Parameter Key - {parameterKey}";
}