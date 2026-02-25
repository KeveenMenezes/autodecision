using Microsoft.AspNetCore.Mvc;
using AutodecisionCore.Services;
using AutodecisionCore.DTOs;

namespace AutodecisionCore.Controllers
{
    [ApiController]
    [Route("/healthcheck")]
    public class HealthCheckController : BaseController
    {
        private readonly IHealthCheckService _service;

        public HealthCheckController(IHealthCheckService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] BaseFilterDto filter) =>
            SuccessWithData(
                (await _service.GetWithFilterAsync(filter))?.Select(
                    p =>
                        new Contracts.Responses.HealthCheckResponse
                        {
                            Name = p.Name,
                            Version = p.Version
                        }
                )
            );

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            if (id == 0)
                return BadRequestWithMessage(log_message: "Id is null");

            return SuccessWithData(
                (await _service.GetWithFilterAsync(new BaseFilterDto(id)))
                    ?.Select(
                        p =>
                            new Contracts.Responses.HealthCheckResponse
                            {
                                Name = p.Name,
                                Version = p.Version
                            }
                    )
                    ?.FirstOrDefault()
            );
        }
    }
}
