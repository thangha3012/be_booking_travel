using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using BookingTravel.Application.DTOs.Tours;
using BookingTravel.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace BookingTravel.Infrastructure.Services
{
    public class ChatService : IChatService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ITourService _tourService;

        public ChatService(HttpClient httpClient, IConfiguration configuration, ITourService tourService)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _tourService = tourService;
        }

        public async Task<string> GetChatResponseAsync(string userMessage)
        {
            var geminiSettings = _configuration.GetSection("GeminiSettings");
            string apiKey = geminiSettings["ApiKey"] ?? "";
            string modelId = geminiSettings["ModelId"] ?? "gemini-1.5-flash";
            string baseUrl = geminiSettings["BaseUrl"] ?? "https://generativelanguage.googleapis.com/v1beta/models/";

            if (string.IsNullOrEmpty(apiKey) || apiKey == "YOUR_GEMINI_API_KEY_HERE")
            {
                return "Chào bạn! Mình là trợ lý AI của Triptopia. Hiện tại chủ nhân của mình chưa cấu hình API Key cho mình, nên mình chưa thể trả lời thông minh được. Vui lòng quay lại sau nhé! 🤖";
            }

            // Lấy context từ database để "dạy" AI
            var tours = await _tourService.GetAllToursAsync(null, null, null);
            var tourList = tours?.Items ?? new List<TourDto>();
            
            var tourInfo = string.Join("\n", tourList.Select(t => 
                $"- {t.Title}: Mã {t.TourCode}, Thời gian {t.Duration}, Giá từ {(t.BasePrice.HasValue ? t.BasePrice.Value.ToString("N0") : "Liên hệ")} VNĐ."));

            string systemInstruction = $@"
            Bạn là trợ lý AI thân thiện tên là 'Triptopia AI'.
            Nhiệm vụ: Tư vấn tour du lịch Việt Nam, giải đáp thắc mắc về lịch trình, giá cả và chính sách.
            Context (Danh sách tour hiện có):
            {tourInfo}
            
            Quy tắc:
            1. Luôn trả lời bằng tiếng Việt thân thiện.
            2. Nếu khách hỏi tour không có trong danh sách, hãy tư vấn dựa trên kiến thức của bạn nhưng lưu ý khách liên hệ hotline 1900 1234.
            3. Trả lời dưới định dạng Markdown.";

            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = systemInstruction + "\n\nCâu hỏi: " + userMessage }
                        }
                    }
                },
                generationConfig = new
                {
                    temperature = 1,
                    topK = 0,
                    topP = 0.95,
                    maxOutputTokens = 8192,
                    stopSequences = new string[] { }
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

                return text ?? "Xin lỗi, mình không tìm thấy câu trả lời.";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi xử lý Chat: {ex.Message}");
                return $"Đã xảy ra lỗi: {ex.Message}. Vui lòng thử lại sau.";
            }
        }
    }
}
