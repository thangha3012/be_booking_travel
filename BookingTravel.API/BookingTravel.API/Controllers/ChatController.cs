using System.Threading.Tasks;
using BookingTravel.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BookingTravel.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;

        public ChatController(IChatService chatService)
        {
            _chatService = chatService;
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] ChatRequest request)
        {
            if (string.IsNullOrEmpty(request.Message))
            {
                return BadRequest("Tin nhắn không được để trống.");
            }

            int? userId = null;
            
            // Log thông tin User Identity để debug (xem trong Output của VS)
            if (User.Identity?.IsAuthenticated == true)
            {
                // Tìm UserId trong các claim phổ biến (Sub, NameIdentifier, Id, uid)
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) 
                               ?? User.FindFirst("sub") 
                               ?? User.FindFirst("id")
                               ?? User.FindFirst("uid");
                
                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int id))
                {
                    userId = id;
                }
            }

            var response = await _chatService.GetChatResponseAsync(request.Message, userId);
            return Ok(new { success = true, reply = response });
        }
    }

    public class ChatRequest
    {
        public string Message { get; set; } = string.Empty;
    }
}
