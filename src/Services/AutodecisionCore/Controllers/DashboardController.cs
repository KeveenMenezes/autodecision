using AutodecisionCore.Commands;
using AutodecisionCore.Services;
using AutodecisionCore.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AutodecisionCore.Controllers
{
    [Route("dashboard")]
    [ApiController]
    public class DashboardController : BaseController
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet]
        [Route("flags/averagetime")]
        public async Task<IActionResult> GetApplicationFlags([FromQuery] int? timeperiod)
        {
            return SuccessWithData(await _dashboardService.GetAverageTimeProcessingFlags(timeperiod));
        }

        [HttpGet]
        [Route("timeperiods")]
        public async Task<IActionResult> GetTimePeriods()
        {
            return SuccessWithData(await _dashboardService.GetTimePeriods());
        }

        [HttpGet]
        [Route("flags/resume")]
        public async Task<IActionResult> GetApplicatGetAmountFlagsByStatusionFlags([FromQuery] int? timeperiod)
        {
            return SuccessWithData(await _dashboardService.GetAmountFlagsByStatus(timeperiod));
        }

        [HttpGet]
        [Route("flags/errors")]
        public async Task<IActionResult> GetFlagsWithError([FromQuery] int? timeperiod)
        {
            return SuccessWithData(await _dashboardService.GetFlagsWithError(timeperiod));
        }

        [HttpGet]
        [Route("applications/averagetime")]
        public async Task<IActionResult> GetProcessingApplicationTime([FromQuery] int? timeperiodId)
        {
            return SuccessWithData(await _dashboardService.GetProcessingApplicationTime(timeperiodId));
        }

        [HttpGet]
        [Route("applications/resume")]
        public async Task<IActionResult> GetAmountApplicationResume([FromQuery] int? timeperiodId)
        {
            return SuccessWithData(await _dashboardService.GetAmountApplicationByStatus(timeperiodId));
        }

        [HttpGet]
        [Route("applications/details")]
        public async Task<IActionResult> GetProcessingApplicationTime([FromQuery] int? timeperiodId, int status)
        {
            return SuccessWithData(await _dashboardService.GetAmountApplicationByStatusDetails(timeperiodId, status));
        }
    }
}
