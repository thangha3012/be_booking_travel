using System.Threading.Tasks;
using BookingTravel.Application.DTOs.Auth;

namespace BookingTravel.Application.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponse> RegisterAsync(RegisterRequest request);
        Task<AuthResponse> LoginAsync(LoginRequest request);
    }
}
