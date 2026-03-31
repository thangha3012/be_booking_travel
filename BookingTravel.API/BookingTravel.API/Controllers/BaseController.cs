using Microsoft.AspNetCore.Mvc;

namespace BookingTravel.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public abstract class BaseController : ControllerBase
    {
        // Standardized Success Response
        protected IActionResult SuccessResult(object? data, string message = "Success", int statusCode = 200)
        {
            return StatusCode(statusCode, new
            {
                success = true,
                message,
                data
            });
        }

        // Standardized Error Response
        protected IActionResult ErrorResult(string message = "Lỗi hệ thống", int statusCode = 400)
        {
            return StatusCode(statusCode, new
            {
                success = false,
                message
            });
        }
    }
}
