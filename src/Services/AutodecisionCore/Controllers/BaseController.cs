using Microsoft.AspNetCore.Mvc;

namespace AutodecisionCore.Controllers
{
    public class BaseController : ControllerBase
    {
        protected IActionResult Success(bool success = true) => Ok(new { success });

        protected IActionResult SuccessWithData(object? data = null, bool success = true) => Ok(new { success, data });

        protected IActionResult SuccessWithMessage(string? message = null, bool success = true) => Ok(new { success, message });

        protected IActionResult NoSuccess(string? message = null, string? log_message = null) => Ok(new { success = false, message, log_message });

        protected new IActionResult BadRequest() => BadRequest(new { success = false });

        protected IActionResult BadRequestWithMessage(string message = "Oops, an error has occured", string? log_message = null) =>
            BadRequest(new { success = false, message, log_message });
    }
}
