using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using BookingTravel.Application.DTOs.Tours;
using BookingTravel.Application.Interfaces;
using BookingTravel.Domain.Enums;
using BookingTravel.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace BookingTravel.Infrastructure.Services
{
    public class ChatService : IChatService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ITourService _tourService;
        private readonly BookingTravelDbContext _context;

        public ChatService(HttpClient httpClient, IConfiguration configuration, ITourService tourService, BookingTravelDbContext context)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _tourService = tourService;
            _context = context;
        }

        public async Task<string> GetChatResponseAsync(string userMessage, int? userId = null)
        {
            var geminiSettings = _configuration.GetSection("GeminiSettings");
            string apiKey = geminiSettings["ApiKey"] ?? "";
            string modelId = geminiSettings["ModelId"] ?? "gemini-2.5-flash";
            string baseUrl = geminiSettings["BaseUrl"] ?? "https://generativelanguage.googleapis.com/v1beta/models/";

            if (string.IsNullOrEmpty(apiKey) || apiKey == "YOUR_GEMINI_API_KEY_HERE")
            {
                return "Chào bạn! Mình là trợ lý AI của Triptopia. Hiện tại chủ nhân của mình chưa cấu hình API Key cho mình, nên mình chưa thể trả lời thông minh được. Vui lòng quay lại sau nhé! 🤖";
            }

            // 1. Lấy thông tin Tour chung
            var tours = await _tourService.GetAllToursAsync(null, null, null);
            var tourList = tours?.Items ?? new List<TourDto>();
            var tourInfo = string.Join("\n", tourList.Select(t => 
                $"- {t.Title}: Mã {t.TourCode}, Thời gian {t.Duration}, Giá từ {(t.BasePrice.HasValue ? t.BasePrice.Value.ToString("N0") : "Liên hệ")} VNĐ."));

            // 2. Lấy thông tin cá nhân hóa (User Context)
            string userContextInfo = "Khách vãng lai (Chưa đăng nhập).";
            if (userId.HasValue)
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId.Value);
                if (user != null)
                {
                    var userBookings = await _context.Bookings
                        .Include(b => b.Tour)
                        .Where(b => b.UserId == userId.Value)
                        .OrderByDescending(b => b.CreatedAt)
                        .ToListAsync();

                    string bookingListInfo = userBookings.Any() 
                        ? string.Join("\n", userBookings.Select(b => {
                            string statusText = b.Status switch {
                                BookingStatus.Pending => "Chờ xử lý",
                                BookingStatus.AwaitingPayment => "Chờ thanh toán",
                                BookingStatus.Confirmed => "Đã xác nhận/Đã thanh toán",
                                BookingStatus.Cancelled => "Đã hủy",
                                BookingStatus.Completed => "Đã hoàn thành",
                                _ => "Không xác định"
                            };
                            return $"- Đơn hàng #{b.Id}: Tour '{b.Tour.Title}', Trạng thái: {statusText}, Tổng tiền: {b.TotalAmount:N0} VNĐ, Ngày đặt: {b.CreatedAt:dd/MM/yyyy}.";
                        }))
                        : "Chưa có đơn hàng nào.";

                    userContextInfo = $@"
                    THÔNG TIN NGƯỜI DÙNG ĐANG CHAT:
                    - Tên: {user.FullName}
                    - Email: {user.Email}
                    - Vai trò: {user.Role} (Quyền hạn: {(user.Role == Role.Admin ? "Quản trị viên toàn hệ thống" : "Khách hàng")})
                    - Lịch sử đặt tour:
                    {bookingListInfo}";

                    // Nếu là Admin, cung cấp thêm số liệu tổng quan hệ thống
                    if (user.Role == Role.Admin)
                    {
                        var totalRevenue = await _context.Bookings.Where(b => b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Completed).SumAsync(b => b.TotalAmount);
                        var totalBookings = await _context.Bookings.CountAsync();
                        var pendingBookings = await _context.Bookings.CountAsync(b => b.Status == BookingStatus.Pending || b.Status == BookingStatus.AwaitingPayment);

                        userContextInfo += $@"
                        DỮ LIỆU THỐNG KÊ HỆ THỐNG (Dành riêng cho Admin):
                        - Tổng doanh thu (Confirmed/Completed): {totalRevenue:N0} VNĐ
                        - Tổng số đơn hàng trên hệ thống: {totalBookings}
                        - Số đơn hàng đang chờ xử lý/thanh toán: {pendingBookings}";
                    }
                }
            }

            string systemInstruction = $@"
            Bạn là 'Triptopia AI' - Trợ lý tư vấn du lịch thông minh, thân thiện của hệ thống Triptopia Travel.
            Thời gian hiện tại: {DateTime.Now:dd/MM/yyyy HH:mm}.

            NGỮ CẢNH HỆ THỐNG:
            {userContextInfo}

            DANH SÁCH TOUR TRÊN HỆ THỐNG:
            {tourInfo}

            NHIỆM VỤ CỦA BẠN:
            1. Hỗ trợ mọi đối tượng: Dù là Khách hàng hay Admin, hãy luôn sẵn sàng tư vấn tour, giải đáp thắc mắc và kiểm tra đơn hàng CÁ NHÂN của họ.
            2. Đối với Admin: Ngoài việc tư vấn tour như người dùng thường, nếu Admin hỏi về tình hình kinh doanh hoặc số liệu hệ thống, hãy sử dụng 'DỮ LIỆU THỐNG KÊ HỆ THỐNG' để báo cáo.
            3. Tra cứu chính xác: Sử dụng danh sách đơn hàng cụ thể trong context để trả lời trạng thái đơn hàng của người đang chat.
            4. Hướng dẫn đặt tour: Luôn sẵn lòng hướng dẫn quy trình đặt và thanh toán.
            5. Tính cách: Niềm nở, chuyên nghiệp, gọi tên người dùng một cách thân mật.

            QUY TẮC:
            - Admin cũng là một khách hàng, có thể đặt tour và có lịch sử đi tour riêng.
            - Không tiết lộ dữ liệu nhạy cảm (mật khẩu, token).
            - Trình bày bằng Markdown đẹp mắt.";

            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = systemInstruction + "\n\nCâu hỏi của khách: " + userMessage }
                        }
                    }
                },
                generationConfig = new
                {
                    temperature = 0.7,
                    topK = 40,
                    topP = 0.95,
                    maxOutputTokens = 2048,
                }
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync($"{baseUrl}{modelId}:generateContent?key={apiKey}", content);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorDetail = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Gemini API Error ({response.StatusCode}): {errorDetail}");
                    return "Hiện tại mình đang bận một chút để chuẩn bị lịch trình tour mới. Bạn vui lòng quay lại sau giây lát nhé! 😊";
                }

                var responseString = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(responseString);
                
                var text = doc.RootElement
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString();

                return text ?? "Xin lỗi, mình gặp chút trục trặc khi suy nghĩ câu trả lời. Bạn có thể hỏi lại được không?";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi xử lý Chat: {ex.Message}");
                return "Hệ thống đang bảo trì phần tư vấn thông minh, bạn vui lòng liên hệ hotline để được hỗ trợ nhanh nhất nhé! 🙏";
            }
        }
    }
}
