using AutodecisionCore.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AutodecisionCore.Controllers
{
    [Route("flags")]
    [ApiController]
    public class FlagsController : BaseController
    {
        private readonly IFlagsService _flagsService;

        public FlagsController(IFlagsService flagsService)
        {
            _flagsService = flagsService;
        }

        [HttpGet]
        public async Task<IActionResult> GetFlagsActive()
        {
            return Ok(await _flagsService.GetAllFlags());
        }

    }
}
