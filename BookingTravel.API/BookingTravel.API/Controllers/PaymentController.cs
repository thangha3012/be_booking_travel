using System;
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

        [HttpPost("create-vnpay-url/{bookingId}")]
        //[Authorize] // Ideally yes, but let's keep it simple or allow frontend to pass it
        public async Task<IActionResult> CreateVnPayUrl(int bookingId)
        {
            // Trong thực tế cần có UserId để kiểm tra lấy đúng Booking, nhưng ở đây demo ta lấy thẳng ID hoặc ignore security auth tạm
            var bookings = await _bookingService.GetAllBookingsAsync();
            var booking = System.Linq.Enumerable.FirstOrDefault(bookings, b => b.Id == bookingId);
            if (booking == null) return NotFound(new { success = false, message = "Không tìm thấy booking" });

            // Thông số VNPay
            var vnp_Returnurl = _configuration["VnpaySettings:ReturnUrl"];
            var vnp_Url = _configuration["VnpaySettings:BaseUrl"];
            var vnp_TmnCode = _configuration["VnpaySettings:TmnCode"];
            var vnp_HashSecret = _configuration["VnpaySettings:HashSecret"];

            var vnpay = new VnPayLibrary();
            vnpay.AddRequestData("vnp_Version", "2.1.0");
            vnpay.AddRequestData("vnp_Command", "pay");
            vnpay.AddRequestData("vnp_TmnCode", vnp_TmnCode);
            long amount = (long)(booking.TotalAmount * 100 * 25000); // Giả sử tỉ giá 25000, hoặc nếu totalAmount đang là VND thì bỏ nhân 25000
            // Đặt tổng tiền là VND, VNPay bắt phải * 100
            // Nãy ở UI thấy dấu $, tức là USD, mình nhân 25000 VND/USD
            vnpay.AddRequestData("vnp_Amount", amount.ToString());

            vnpay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
            vnpay.AddRequestData("vnp_CurrCode", "VND");
            vnpay.AddRequestData("vnp_IpAddr", VnPayLibrary.GetIpAddress(HttpContext));
            vnpay.AddRequestData("vnp_Locale", "vn");
            vnpay.AddRequestData("vnp_OrderInfo", $"Thanh toan don hang {bookingId}");
            vnpay.AddRequestData("vnp_OrderType", "other"); //default value: other
            vnpay.AddRequestData("vnp_ReturnUrl", vnp_Returnurl);
            vnpay.AddRequestData("vnp_TxnRef", bookingId.ToString()); // Mã tham chiếu (mã đơn hàng)

            string paymentUrl = vnpay.CreateRequestUrl(vnp_Url, vnp_HashSecret);

            return Ok(new { success = true, url = paymentUrl });
        }

        [HttpGet("vnpay-return")]
        public async Task<IActionResult> VnPayReturn()
        {
            if (Request.Query.Count > 0)
            {
                var vnp_HashSecret = _configuration["VnpaySettings:HashSecret"];
                var vnPayData = Request.Query;
                VnPayLibrary vnpay = new VnPayLibrary();

                foreach (var s in vnPayData)
                {
                    if (!string.IsNullOrEmpty(s.Key) && s.Key.StartsWith("vnp_"))
                    {
                        vnpay.AddResponseData(s.Key, s.Value.ToString());
                    }
                }

                int bookingId = Convert.ToInt32(vnpay.GetResponseData("vnp_TxnRef"));
                long vnp_Amount = Convert.ToInt64(vnpay.GetResponseData("vnp_Amount")) / 100;
                string vnp_ResponseCode = vnpay.GetResponseData("vnp_ResponseCode");
                string vnp_SecureHash = Request.Query["vnp_SecureHash"];

                bool checkSignature = vnpay.ValidateSignature(vnp_SecureHash, vnp_HashSecret);
                if (checkSignature)
                {
                    if (vnp_ResponseCode == "00")
                    {
                        // Thanh toán thành công
                        await _bookingService.UpdateBookingStatusAsync(bookingId, BookingTravel.Domain.Enums.BookingStatus.Confirmed);
                        // Redirect về frontend
                        return Redirect($"{_configuration["VnpaySettings:ReturnUrl"]}?success=true&bookingId={bookingId}");
                    }
                    else
                    {
                        // Thanh toán lỗi hoặc cancel
                        return Redirect($"{_configuration["VnpaySettings:ReturnUrl"]}?success=false&bookingId={bookingId}&code={vnp_ResponseCode}");
                    }
                }
                else
                {
                    return Redirect($"{_configuration["VnpaySettings:ReturnUrl"]}?success=false&error=invalid_signature");
                }
            }
            return Redirect($"{_configuration["VnpaySettings:ReturnUrl"]}?success=false");
        }
    }
}
