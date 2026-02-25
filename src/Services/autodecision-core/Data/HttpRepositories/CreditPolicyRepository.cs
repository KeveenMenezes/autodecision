using AutodecisionCore.Contracts.Constants;
using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionCore.Data.HttpRepositories.DTOs;
using AutodecisionCore.Data.HttpRepositories.Interfaces;
using AutodecisionCore.DTOs;
using BMGMoney.SDK.V2.Http;

namespace AutodecisionCore.Data.HttpRepositories
{
    public class CreditPolicyRepository : ICreditPolicyRepository
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpService _httpService;
        private readonly ILogger<CreditPolicyRepository> _logger;

        public CreditPolicyRepository(IConfiguration configuration, IHttpService httpService, ILogger<CreditPolicyRepository> logger)
        {
            _configuration = configuration;
            _httpService = httpService;
            _logger = logger;
        }

        public async Task<CreditPolicyApiResultDTO> GetCreditPolicyInfoAsync(string employerKey, string productKey, string stateAbbreviation)
        {
            try
            {
                var url = _configuration["Apis:CreditPolicyApi"] + $"/credit-policy?employerKey={employerKey}&productKey={productKey}&stateAbbreviation={stateAbbreviation}";

                var result = await _httpService.GetAsync<CreditPolicyApiResultDTO>(url);

                if (!result.Success)
                {
                    _logger.LogWarning($"Error while trying to get credit policy, response_code: {result.StatusCode}");
                    return new CreditPolicyApiResultDTO();
                }
                return result.Response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error when trying to get credit policy information. | Error: {ex.Message}");
                throw;
            }
        }

        public async Task<ResultDTO<List<EmployerRulesApiResultDTO>>> GetEmployerRulesInfoAsync(string groupKey, string ruleKey = null)
        {
            try
            {
                var url = _configuration["Apis:EmployerRules"] + $"/rules?GroupKey={groupKey}&RuleKey={ruleKey}";

                var result = await _httpService.GetAsync<ResultDTO<List<EmployerRulesApiResultDTO>>>(url);

                if (!result.Success)
                {
                    _logger.LogWarning($"Error while trying to get employer rules, response_code: {result.StatusCode}");
                    return new ResultDTO<List<EmployerRulesApiResultDTO>>();
                }
                return result.Response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error when trying to get employer rules information. | Error: {ex.Message}");
                throw;
            }
        }
        public async Task<CreditPolicy> GetCreditPolicyInfo(string employerKey, string productKey, string stateAbbreviation, string applicationType)
        {
            try
            {
                var creditPolicy = await GetCreditPolicyInfoAsync(employerKey, productKey, stateAbbreviation);
                var ruleGroupKey = creditPolicy.CreditPolicy.RuleGroupKey;

                if (applicationType.Equals(ApplicationType.Refi))
                    ruleGroupKey = creditPolicy.CreditPolicy.RuleGroupRefiKey;

                var employerRules = await GetEmployerRulesInfoAsync(ruleGroupKey);
                var adaptEmployerRules = new List<EmployerRulesItem>();

                foreach (var item in employerRules.Data)
                {
                    adaptEmployerRules.Add(new EmployerRulesItem()
                    {
                        Key = item.RuleKey,
                        Default = item.DefaultValue,
                        Max = item.MaxValue,
                        Min = item.MinValue,
                        Required = item.Required,
                        ValueType = item.ValueType
                    });
                }
                return new CreditPolicy() { EmployerRules = new EmployerRules() { EmployerRulesItems = adaptEmployerRules } };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error when trying to get Credit Policy for EmployerKey: {employerKey ?? "Empty"} | ProductKey: {productKey ?? "Empty"} | StateAbbreviation: {stateAbbreviation} | ApplicationType: {applicationType} | Error: {ex.Message}");
                return new CreditPolicy() { EmployerRules = new EmployerRules() { EmployerRulesItems =  new List<EmployerRulesItem>() } };
            }
        }
    }
}
