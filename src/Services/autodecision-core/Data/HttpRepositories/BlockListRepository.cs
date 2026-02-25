using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionCore.Data.HttpRepositories.DTOs;
using AutodecisionCore.Data.HttpRepositories.Interfaces;
using AutoMapper;
using BMGMoney.SDK.V2.Http;

namespace AutodecisionCore.Data.HttpRepositories
{
    public class BlockListRepository : IBlockListRepository
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpService _httpService;
        private readonly ILogger<BlockListRepository> _logger;

        public BlockListRepository(
            IConfiguration configuration,
            IHttpService httpService,
            ILogger<BlockListRepository> logger)
        {
            _configuration = configuration;
            _httpService = httpService;
            _logger = logger;
        }

        public async Task<BlockList?> GetBlockListInfo(int customerId)
        {
            try
            {
                var url = _configuration["Apis:CustomerInfoApi"] + $"/block-list/autodecision-info/{customerId}";

                var result = await _httpService.GetAsync<BlockListDTO>(url);
                if (!result.Success)
                {
                    _logger.LogWarning($"CustomerId: {customerId} - couldn't get the Block List info, response_code: {result.StatusCode}");
                    return null;
                }
                var config = new MapperConfiguration(c =>
                {
                    c.CreateMap<BlockListDTO, BlockList>()
                        .ConvertUsing(src => MapBlockListDto(src));
                });
                var mapper = new Mapper(config);

                return mapper.Map<BlockList>(result.Response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error when trying to get Block List info for CustomerId: {customerId} | Error: {ex.Message}");
                throw;
            }
        }

        private BlockList? MapBlockListDto(BlockListDTO src)
        {
            if (src.CreatedAt == null && src.CreatedBy == null && src.CreatedNote == null && src.Reason == null)
                return null;

            return new BlockList
            {
                CreatedAt = src.CreatedAt.Value,
                CreatedBy = src.CreatedBy,
                CreatedNote = src.CreatedNote,
                Reason = src.Reason
            };
        }
    }

}
