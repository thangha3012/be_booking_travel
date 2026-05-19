using System.Threading.Tasks;

namespace BookingTravel.Application.Interfaces
{
    public interface IChatService
    {
        Task<string> GetChatResponseAsync(string userMessage, int? userId = null);
    }
}
