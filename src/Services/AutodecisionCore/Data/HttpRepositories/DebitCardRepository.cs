using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionCore.Data.HttpRepositories.DTOs;
using AutodecisionCore.Data.HttpRepositories.Interfaces;
using AutoMapper;
using BMGMoney.SDK.V2.Http;

namespace AutodecisionCore.Data.HttpRepositories
{
    public class DebitCardRepository : IDebitCardRepository
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpService _httpService;
        private readonly ILogger<DebitCardRepository> _logger;

        public DebitCardRepository(IConfiguration configuration, IHttpService httpService, ILogger<DebitCardRepository> logger)
        {
            _configuration = configuration;
            _httpService = httpService;
            _logger = logger;
        }

        public async Task<DebitCard> BinNameLink(DebitCard debitCard, string bankName)
        {
            try
            {
                var url = _configuration["Apis:DebitCardApi"] + $"/api/binNameBankNameLink?binName={debitCard.CardBinEmissor}&bankName={bankName}";

                var result = await _httpService.GetAsync<ResponseBinNameLink>(url);

                if (!result.Success)
                {
                    _logger.LogWarning($"CustomerId: {debitCard.CustomerId} couldn't get the the validation of BinName and BankName, response_code: {result.StatusCode}");
                    return new DebitCard();
                }
                if (result.Response != null && result.Response.Data != null)
                {
                    debitCard.IsConnected = result.Response.Data.IsConnected;
                }

                return debitCard;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error when trying to get validation of BinName and BankName for CustomerId: {debitCard.CustomerId} | Error: {ex.Message}");
                throw;
            }
        }

        public async Task<DebitCard> GetAccountsByCustomerId(int customerId)
        {
            var response = new DebitCard();
            try
            {
                var url = _configuration["Apis:DebitCardApi"] + $"/accounts?customerId={customerId}";

                var result = await _httpService.GetAsync<AccountsCustomerReturn>(url);
                if (!result.Success)
                {
                    _logger.LogWarning($"couldn't get informations of debit card, response_code: {result.StatusCode}");
                    return response;
                }
                var config = new MapperConfiguration(c => c.CreateMap<AccountsDTO, DebitCard>());
                var mapper = new Mapper(config);
                if (result.Response.Data.List.Any(x => x.Active))
                {
                    response = mapper.Map<DebitCard>(result.Response.Data.List.FirstOrDefault(x => x.Active));
                }
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error when trying to get validation of information of Debit Card for CustomerId: {customerId} | Error: {ex.Message}");
                throw;
            }
        }
    }
}
