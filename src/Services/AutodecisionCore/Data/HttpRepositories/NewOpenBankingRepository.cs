using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionCore.Data.HttpRepositories.DTOs;
using AutodecisionCore.Data.HttpRepositories.Interfaces;
using AutodecisionCore.DTOs;
using AutodecisionCore.Extensions;
using AutodecisionCore.Utils;
using Newtonsoft.Json;

namespace AutodecisionCore.Data.HttpRepositories;

public class NewOpenBankingRepository : INewOpenBankingRepository
{
    private readonly ILogger<NewOpenBankingRepository> _logger;
    private readonly string _baseUrl;

    public NewOpenBankingRepository(ILogger<NewOpenBankingRepository> logger)
    {
        _logger = logger;
        _baseUrl = Environment.GetEnvironmentVariable("API_URL_NEW_OPEN_BANKING") + "/api/v1/";
    }
    public async Task<OpenBanking> GetNewOpenBanking(int customerId)
    {
        try
        {
            Dictionary<string, string> headers = Token.GetHeaders();

            OpenBanking? openBankingResult = await FetchOpenBanking(customerId, headers);
            if (openBankingResult == null)
            {
                _logger.LogWarning($"CustomerId: {customerId} - couldn't get the open banking information");
                return new OpenBanking();
            }             
            return openBankingResult;
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"CustomerId: {customerId} - couldn't get the open banking information, error {ex}");
            return new OpenBanking();
        }
    }

    private async Task<OpenBanking?> FetchOpenBanking(int customerId, Dictionary<string, string> headers)
    {
        HttpClient client = new HttpClient();
        HttpResponseMessage response = await client.SendAsync(CreateHttpRequestWithHeaders(headers, GetUrlToFetchOpenBanking(customerId)));
        client.DefaultRequestHeaders.Clear();

        if (!response.IsSuccessStatusCode)
            return null;

        var getOpenBankingResult = JsonConvert.DeserializeObject<ApiResultDto<NewOpenBanking>>(await response.Content.ReadAsStringAsync());
        if (getOpenBankingResult == null || getOpenBankingResult.Data.ActiveAccounts.Count == 0)
            return null;

        var result = new OpenBanking() { Connections = new List<OpenBankingConnections>()};

        foreach(var account in getOpenBankingResult.Data.ActiveAccounts)
        {
            result.Connections.Add(new OpenBankingConnections
            {
                AccountNumber = account.AccountNumber,
                RoutingNumber = account.RoutingNumber,
                Type = account.AccountType,
                Name = string.Empty,
                IsDefault = account.IsDefault.GetValueOrDefault(),
                CreatedAt = account.CreatedAt,
                UpdatedAt = null,
                Vendor = ((VendorIds)getOpenBankingResult.Data.VendorId).ToString()
            });
        }   

        return result;
    }

    private string GetUrlToFetchOpenBanking(int customerId)
    {
        string url = _baseUrl + $"accounts/active-accounts/{customerId}";
        return url.Replace(",", ".");
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