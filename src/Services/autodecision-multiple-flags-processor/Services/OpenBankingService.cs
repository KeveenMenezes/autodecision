using AutodecisionMultipleFlagsProcessor.DTOs;
using AutodecisionMultipleFlagsProcessor.Services.Interfaces;
using AutodecisionMultipleFlagsProcessor.Utility;
using BMGMoney.SDK.V2.Http;

public class OpenBankingService : IOpenBankingService
{
    private readonly IHttpService _httpService;
    private readonly string _newOpenBankingUrl;

    public OpenBankingService(IHttpService httpService)
    {
        _httpService = httpService;
        _newOpenBankingUrl = Environment.GetEnvironmentVariable("API_URL_NEW_OPEN_BANKING") + "/api/v1/";
    }

    public async Task<DailyReceivingsDTO> GetDailyReceivings(int customerId)
    {
        string url = _newOpenBankingUrl + $"transactions/daily-receivings/{customerId}";
        var result = await _httpService.GetAsync<DailyReceivingsDTO>(url, Token.GetHeaders());
        return result.Response;
    }
}