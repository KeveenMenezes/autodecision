using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionCore.Data.HttpRepositories.DTOs;
using AutodecisionCore.Data.HttpRepositories.Interfaces;
using AutodecisionCore.Utils;
using Newtonsoft.Json;

namespace AutodecisionCore.Data.HttpRepositories
{
    public class CreditRiskRepository : ICreditRiskRepository
    {
        private readonly ILogger<CreditRiskRepository> _logger;
        private readonly string _creditRiskUrl;

        public CreditRiskRepository(ILogger<CreditRiskRepository> logger)
        {
            _logger = logger;
            _creditRiskUrl = Environment.GetEnvironmentVariable("API_URL_CREDIT_RISK") + "/api/v1/";
        }

        public async Task<ApplicationScore> GetApplicationScore(string loanNumber)
        {
            try
            {   
                var url = _creditRiskUrl + $"application-score/score/{loanNumber}";
                Dictionary<string, string> headers = Token.GetHeaders();

                HttpClient client = new HttpClient();
                HttpResponseMessage response = await client.SendAsync(CreateHttpRequestWithHeaders(headers, url));
                client.DefaultRequestHeaders.Clear();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning($"LoanNumber: {loanNumber} - couldn't get the Credit Risk information, response_code: {response.StatusCode}");
                    return new ApplicationScore();
                }

                var result = JsonConvert.DeserializeObject<ApiResultDto<ApplicationScore>>(await response.Content.ReadAsStringAsync());
                if (result is null)
                    return new ApplicationScore();

                return result.Data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error while trying to get Credit Risk Info. | LoanNumber: {loanNumber} | Error: {ex.Message}");
                throw;
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
    }
}
