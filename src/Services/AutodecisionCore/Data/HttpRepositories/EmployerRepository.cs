using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionCore.Contracts.ViewModels.Helpers;
using AutodecisionCore.Data.HttpRepositories.DTOs;
using AutodecisionCore.Data.HttpRepositories.Interfaces;
using AutoMapper;
using BMGMoney.SDK.V2.Http;

namespace AutodecisionCore.Data.HttpRepositories
{
    public class EmployerRepository : IEmployerRepository
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpService _httpService;
        private readonly ILogger<EmployerRepository> _logger;

        public EmployerRepository(IConfiguration configuration, IHttpService httpService, ILogger<EmployerRepository> logger)
        {
            _configuration = configuration;
            _httpService = httpService;
            _logger = logger;
        }
        public async Task<Employer> GetEmployerAsync(int employerId)
        {
            try
            {
                var url = _configuration["Apis:CustomerInfoApi"] + $"/employer-infos/employer?employerId={employerId}";

                var result = await _httpService.GetAsync<EmployerDTO>(url);
                if (!result.Success)
                {
                    _logger.LogWarning($"Employer Id: {employerId} - couldn't get the Employer, response_code: {result.StatusCode}");
                    return new Employer();
                }

                var config = new MapperConfiguration(c =>
                c.CreateMap<EmployerDTO, Employer>()
                        .ConvertUsing(src => MapEmployerDto(src)));

                var mapper = new Mapper(config);

                return mapper.Map<Employer>(result.Response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Employer Id: {employerId} - couldn't get the Employer, response_code: {ex.Message}");
                throw;
            }
        }

        private static Employer? MapEmployerDto(EmployerDTO src)
        {
            if (src == null) return null;

            return new Employer()
            {
                Id = src.id,
                EmployerName = src.name,
                Program = src.program,
                SubProgramId = src.sub_program_id,
                DueDiligenceStatus = src.due_diligence_status
            };
        }
    }
}
