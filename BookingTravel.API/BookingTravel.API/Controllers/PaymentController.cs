using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using BookingTravel.API.Helpers;
using BookingTravel.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace BookingTravel.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IBookingService _bookingService;

        public PaymentController(IConfiguration configuration, IBookingService bookingService)
        {
            _configuration = configuration;
            _bookingService = bookingService;
        }

        // Tạo yêu cầu thanh toán sang cổng ZaloPay
        [HttpPost("create-zalopay-order/{bookingId}")]
        public async Task<IActionResult> CreateZaloPayOrder(int bookingId)
        {
            var bookings = await _bookingService.GetAllBookingsAsync();
            var booking = System.Linq.Enumerable.FirstOrDefault(bookings, b => b.Id == bookingId);
            if (booking == null) return NotFound(new { success = false, message = "Không tìm thấy booking" });

            // Lấy giá trị thực từ đơn hàng
            long amount = (long)booking.TotalAmount;
            
            // ZaloPay Sandbox (AppID 2553) giới hạn tối đa 10.000.000 VND mỗi giao dịch
            // Trong môi trường Production, bỏ dòng cap này
            if (amount > 10000000) amount = 10000000; 
            
            if (amount <= 0) amount = 1000; // Giá trị tối thiểu cho giao dịch hợp lệ

            var embed_data = new { redirecturl = _configuration["ZaloPaySettings:ReturnUrl"] };
            var item = new[] { new { itemid = bookingId.ToString(), itemname = "Trip Booking", itemprice = amount, itemquantity = 1 } };

            var param = new Dictionary<string, string>
            {
                { "app_id", _configuration["ZaloPaySettings:AppId"] },
                { "app_user", "BookingUser" },
                { "app_time", DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString() },
                { "amount", amount.ToString() },
                { "app_trans_id", DateTime.Now.ToString("yyMMdd") + "_" + bookingId + "_" + Guid.NewGuid().ToString().Substring(0, 6) },
                { "embed_data", JsonSerializer.Serialize(embed_data) },
                { "item", JsonSerializer.Serialize(item) },
                { "description", "Thanh toan don hang #" + bookingId },
                { "bank_code", "" }
            };

            string data = param["app_id"] + "|" + param["app_trans_id"] + "|" + param["app_user"] + "|" + param["amount"] + "|" + param["app_time"] + "|" + param["embed_data"] + "|" + param["item"];
            param.Add("mac", ZaloPayHelper.CreateMac(data, _configuration["ZaloPaySettings:Key1"]));

            using var client = new HttpClient();
            var content = new FormUrlEncodedContent(param);
            var response = await client.PostAsync(_configuration["ZaloPaySettings:BaseUrl"], content);
            var responseString = await response.Content.ReadAsStringAsync();
            var responseData = JsonSerializer.Deserialize<JsonElement>(responseString);

            if (responseData.GetProperty("return_code").GetInt32() == 1)
            {
                return Ok(new { success = true, url = responseData.GetProperty("order_url").GetString() });
            }
            else
            {
                return BadRequest(new { success = false, message = responseData.GetProperty("return_message").GetString() });
            }
        }

        // Tiếp nhận và xử lý kết quả phản hồi từ ZaloPay sau khi thanh toán
        [HttpGet("zalopay-return")]
        public async Task<IActionResult> ZaloPayReturn()
        {
            if (Request.Query.Count > 0)
            {
                try
                {
                    // Lấy tất cả tham số có thể có từ ZaloPay (hỗ trợ cả status và returncode)
                    string status = Request.Query.ContainsKey("status") ? Request.Query["status"].ToString() : "";
                    if (string.IsNullOrEmpty(status)) status = Request.Query.ContainsKey("returncode") ? Request.Query["returncode"].ToString() : "";
                    
                    string apptransid = Request.Query.ContainsKey("apptransid") ? Request.Query["apptransid"].ToString() : "";
                    
                    // 1: Thành công, 2: Thất bại (theo tài liệu Redirect của ZaloPay)
                    if (status == "1")
                    {
                        if (!string.IsNullOrEmpty(apptransid))
                        {
                            // apptransid format: yyMMdd_bookingId_guid
                            var parts = apptransid.Split('_');
                            if (parts.Length >= 2 && int.TryParse(parts[1], out int bookingId))
                            {
                                var success = await _bookingService.UpdateBookingStatusAsync(bookingId, BookingTravel.Domain.Enums.BookingStatus.Confirmed);
                                if (success)
                                {
                                    return Ok(new { success = true, message = "Thanh toán và cập nhật đơn hàng thành công" });
                                }
                            }
                        }
                        return BadRequest(new { success = false, message = "Không xác định được mã đơn hàng từ apptransid: " + apptransid });
                    }
                    else
                    {
                        return BadRequest(new { success = false, message = "Giao dịch không thành công hoặc bị hủy. Status: " + status });
                    }
                }
                catch (Exception ex)
                {
                    return BadRequest(new { success = false, message = "Lỗi xử lý: " + ex.Message });
                }
            }
            return BadRequest(new { success = false, message = "Thiếu dữ liệu giao dịch" });
        }
    }
}
