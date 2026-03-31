using System.Threading.Tasks;
using BookingTravel.Application.DTOs.Auth;

namespace BookingTravel.Application.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponse> RegisterAsync(RegisterRequest request);
        Task<AuthResponse> LoginAsync(LoginRequest request);
        Task<bool> ForgotPasswordAsync(ForgotPasswordRequest request);
        Task<bool> VerifyOtpAsync(VerifyOtpRequest request);
        Task<bool> ResetPasswordAsync(ResetPasswordRequest request);
    }
}
