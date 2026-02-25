using AutodecisionCore.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AutodecisionCore.Controllers
{
    [Route("decline_reason_flags")]
    [ApiController]
    public class DeclineReasonFlagsController : BaseController
    {
        private readonly IDeclineReasonFlagsService _declineReasonFlagsService;

        public DeclineReasonFlagsController(IDeclineReasonFlagsService declineReasonFlagsService)
        {
            _declineReasonFlagsService = declineReasonFlagsService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllActive()
        {
            return Ok(await _declineReasonFlagsService.GetAllActiveAsync());
        }

    }
}
