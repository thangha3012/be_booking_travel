using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using BookingTravel.Application.DTOs.Auth;
using BookingTravel.Application.Helpers;
using BookingTravel.Application.Interfaces;
using BookingTravel.Domain.Entities;
using BookingTravel.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace BookingTravel.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly BookingTravelDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;

        public AuthService(BookingTravelDbContext context, IConfiguration configuration, IEmailService emailService)
        {
            _context = context;
            _configuration = configuration;
            _emailService = emailService;
        }

        // Xử lý yêu cầu quên mật khẩu: tạo OTP và gửi email xác nhận
        public async Task<bool> ForgotPasswordAsync(ForgotPasswordRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null)
            {
                // Báo lỗi trực tiếp cho người dùng biết email nhập sai
                throw new Exception("Email không tồn tại trong hệ thống. Vui lòng kiểm tra lại.");
            }

            // Sinh mã ngẫu nhiên 6 số
            string resetToken = new Random().Next(100000, 999999).ToString();
            user.ResetPasswordToken = resetToken;
            user.ResetPasswordExpiry = DateTime.UtcNow.AddMinutes(15);
            
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            // Gửi qua Email Service
            await _emailService.SendEmailAsync(
                request.Email, 
                "MÃ XÁC NHẬN OTP - ĐẶT LẠI MẬT KHẨU", 
                $"Xin chào {user.FullName},\n\nMã OTP xác nhận đặt lại mật khẩu của bạn là: {resetToken}\nMã OTP này có hiệu lực trong vòng 15 phút.\nNếu bạn không yêu cầu, vui lòng bỏ qua email này.");

            return true;
        }

        // Xác thực mã OTP người dùng nhập vào
        public async Task<bool> VerifyOtpAsync(VerifyOtpRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null || user.ResetPasswordToken != request.Token || user.ResetPasswordExpiry < DateTime.UtcNow)
            {
                throw new Exception("Mã OTP không hợp lệ hoặc đã hết hạn.");
            }

            // OTP đúng, cho phép tiếp tục đổi mật khẩu.
            return true;
        }

        // Đặt lại mật khẩu mới sau khi đã xác thực OTP thành công
        public async Task<bool> ResetPasswordAsync(ResetPasswordRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null || user.ResetPasswordToken != request.Token || user.ResetPasswordExpiry < DateTime.UtcNow)
            {
                throw new Exception("Mã xác nhận không hợp lệ hoặc đã hết hạn.");
            }

            user.PasswordHash = PasswordHelper.HashPassword(request.NewPassword);
            user.ResetPasswordToken = null;
            user.ResetPasswordExpiry = null;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return true;
        }

        // Đăng ký tài khoản người dùng mới
        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            {
                throw new Exception("Email đã được sử dụng.");
            }

            if (request.Password != request.ConfirmPassword)
            {
                throw new Exception("Mật khẩu xác nhận không khớp.");
            }

            var newUser = new User
            {
                Email = request.Email,
                FullName = request.FullName,
                Phone = request.Phone,
                PasswordHash = PasswordHelper.HashPassword(request.Password),
                Role = Domain.Enums.Role.Customer,
                IsActive = true
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            return GenerateAuthResponse(newUser);
        }

        // Đăng nhập hệ thống và trả về Token xác thực
        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null || !PasswordHelper.VerifyPassword(request.Password, user.PasswordHash))
            {
                throw new Exception("Email hoặc mật khẩu không chính xác.");
            }

            if (!user.IsActive)
            {
                throw new Exception("Tài khoản của bạn đã bị khóa.");
            }

            return GenerateAuthResponse(user);
        }

        // Tạo chuỗi JWT Token chứa thông tin người dùng (Claims)
        private AuthResponse GenerateAuthResponse(User user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["Secret"];
            var issuer = jwtSettings["Issuer"];
            var audience = jwtSettings["Audience"];
            var expiryMinutes = int.Parse(jwtSettings["ExpiryMinutes"]!);

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(secretKey!);
            
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(expiryMinutes),
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var jwtToken = tokenHandler.WriteToken(token);

            // TODO: Generate and save refresh token (if needed)

            return new AuthResponse
            {
                AccessToken = jwtToken,
                RefreshToken = "dummy-refresh-token",
                Email = user.Email,
                FullName = user.FullName,
                RoleId = (int)user.Role
            };
        }
    }
}
