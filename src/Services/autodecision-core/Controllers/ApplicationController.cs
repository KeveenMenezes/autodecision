using AutodecisionCore.Commands;
using AutodecisionCore.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AutodecisionCore.Controllers
{
    [Route("application")]
    [ApiController]
    public class ApplicationController : BaseController
    {
        private readonly IApplicationCoreService _applicationCoreService;

        public ApplicationController(IApplicationCoreService applicationCoreService)
        {
            _applicationCoreService = applicationCoreService;
        }

        [HttpGet]
        [Route("flags")]
        public async Task<IActionResult> GetApplicationFlags([FromQuery] List<string> loanNumbers)
        {
            return Ok(await _applicationCoreService.GetFlagsByLoanNumbers(loanNumbers));
        }

        [HttpGet]
        [Route("{loanNumber}/flags")]
        public async Task<IActionResult> GetFlagsByLoanNumber([FromRoute] string loanNumber)
        {
            return SuccessWithData(await _applicationCoreService.GetFlagsByLoanNumber(loanNumber));
        }

        [HttpGet]
        [Route("{loanNumber}/flags/{flagCode}")]
        public async Task<IActionResult> GetFlagByLoanNumberAndFlagDode([FromRoute] string loanNumber, [FromRoute] string flagCode)
        {
            return SuccessWithData(await _applicationCoreService.GetFlagsByLoanNumberAndFlagCode(loanNumber,flagCode));
        }

        [HttpPut]
        [Route("{loanNumber}/flags/{flagCode}/approve")]
        public async Task<IActionResult> ApproveFlag([FromRoute] string loanNumber, [FromRoute]string flagCode, [FromBody] ApproveFlagCommand command )
        {
            try
            {
                return Ok(await _applicationCoreService.ApproveFlag(loanNumber, flagCode, command));
            }
            catch (Exception)
            {
                return BadRequest();
            }
            
        }

		[HttpPut]
		[Route("{loanNumber}/openforchanges")]
		public async Task<IActionResult> OpenForChanges([FromRoute] string loanNumber, [FromBody] string user)
		{
			return Ok(await _applicationCoreService.OpenForChanges(loanNumber, user));
		}
	}
}
