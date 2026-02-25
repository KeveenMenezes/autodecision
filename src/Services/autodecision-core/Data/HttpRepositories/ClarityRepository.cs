using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionCore.Data.HttpRepositories.DTOs;
using AutodecisionCore.Data.HttpRepositories.Interfaces;
using AutoMapper;
using BMGMoney.SDK.V2.Http;

namespace AutodecisionCore.Data.HttpRepositories
{
    public class ClarityRepository: IClarityRepository
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpService _httpService;
        private readonly ILogger<ClarityRepository> _logger;

        public ClarityRepository(
            IConfiguration configuration,
            IHttpService httpService,
            ILogger<ClarityRepository> logger)
        {
            _configuration = configuration;
            _httpService = httpService;
            _logger = logger;
        }

        public async Task<Clarity?> GetClarityInfo(string loanNumber)
        {
            try
            {
                var url = _configuration["Apis:CustomerInfoApi"] + $"/clarity/autodecision-info/{loanNumber}";

                var result = await _httpService.GetAsync<ClarityDTO>(url);
                if (!result.Success)
                {
                    _logger.LogWarning($"LoanNumber: {loanNumber} - couldn't get the Clarity information, response_code: {result.StatusCode}");
                    return null;
                }

                var config = new MapperConfiguration(c =>
                {
                    c.CreateMap<ClarityDTO, Clarity>()
                        .ConvertUsing(src => MapClarityDto(src));
                });

                var mapper = new Mapper(config);

                return mapper.Map<Clarity>(result.Response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error while trying to get Clarity Info. | Error: {ex.Message}");
                throw;
            }
        }
        
        public  bool IsClarityValid(Clarity? clarity) =>
            clarity is { DobMatchResult: "Match" } &&
            clarity.DobMatchDescription == "Full DOB available and matched input (within +/- 1 year if month.day exact)" &&
            clarity.NameAddressMatchConfidence == 5 &&
            clarity.NameAddressMatchDescription == "Exact match on first and last name; Exact match on address" &&
            clarity.SSNNameAddressMatchConfidence == 5 &&
            clarity.SSNNameAddressMatchDescription == "Exact SSN match, Exact Name match, Exact Address match";

        private Clarity? MapClarityDto(ClarityDTO src)
        {
            if (src.DobMatchDescription == null && src.DobMatchResult == null && src.NameAddressMatchDescription == null && src.NameAddressMatchConfidence == null && src.SSNNameAddressMatchDescription == null && src.SSNNameAddressMatchConfidence == null && src.RequestHash == null)
                return null;

            var NameAddressMatchConfidence = src.NameAddressMatchConfidence.HasValue ? src.NameAddressMatchConfidence.Value : 0;
            var SSNNameAddressMatchConfidence = src.SSNNameAddressMatchConfidence.HasValue ? src.SSNNameAddressMatchConfidence.Value : 0;
            return new Clarity
            {
                DobMatchDescription = src.DobMatchDescription,
                DobMatchResult = src.DobMatchResult,
                NameAddressMatchDescription = src.NameAddressMatchDescription,
                NameAddressMatchConfidence = NameAddressMatchConfidence,
                SSNNameAddressMatchDescription = src.SSNNameAddressMatchDescription,
                SSNNameAddressMatchConfidence = SSNNameAddressMatchConfidence,
                RequestHash = src.RequestHash,
                OFACHit = src.OFACHit
            };
        }
    }
}
