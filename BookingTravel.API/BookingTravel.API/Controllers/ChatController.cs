using System.Threading.Tasks;
using BookingTravel.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

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

            var response = await _chatService.GetChatResponseAsync(request.Message);
            return Ok(new { success = true, reply = response });
        }
    }

    public class ChatRequest
    {
        public string Message { get; set; } = string.Empty;
    }
}
