using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionCore.Data.HttpRepositories.DTOs;
using AutodecisionCore.Data.HttpRepositories.Interfaces;
using AutoMapper;
using BMGMoney.SDK.V2.Http;

namespace AutodecisionCore.Data.HttpRepositories
{
    public class WhiteListRepository : IWhiteListRepository
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpService _httpService;
        private readonly ILogger<WhiteListRepository> _logger;

        public WhiteListRepository(
            IConfiguration configuration,
            IHttpService httpService,
            ILogger<WhiteListRepository> logger)
        {
            _configuration = configuration;
            _httpService = httpService;
            _logger = logger;
        }

        public async Task<WhiteList?> GetWhiteListInfo(int customerId)
        {
            try
            {
                var url = _configuration["Apis:CustomerInfoApi"] + $"/white-list/autodecision-info/{customerId}";

                var result = await _httpService.GetAsync<WhiteListDTO>(url);
                if (!result.Success)
                {
                    _logger.LogWarning($"CustomerId: {customerId} - couldn't get the White List info, response_code: {result.StatusCode}");
                    return null;
                }

                var config = new MapperConfiguration(c =>
                {
                    c.CreateMap<WhiteListDTO, WhiteList>()
                        .ConvertUsing(src => MapWhiteListDto(src));
                });

                var mapper = new Mapper(config);

                return mapper.Map<WhiteList>(result.Response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error when trying to get White List info for CustomerId: {customerId} | Error: {ex.Message}");
                throw;
            }
        }

        private WhiteList? MapWhiteListDto(WhiteListDTO src)
        {
            if (src.CreatedAt == null && src.CreatedBy == null && src.CreatedNote == null && src.Reason == null)
                return null;

            return new WhiteList
            {
                CreatedAt = src.CreatedAt.Value,
                CreatedBy = src.CreatedBy,
                CreatedNote = src.CreatedNote,
                Reason = src.Reason
            };
        }
    }
}
