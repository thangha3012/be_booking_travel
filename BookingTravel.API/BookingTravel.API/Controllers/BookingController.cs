using System;
using System.Security.Claims;
using System.Threading.Tasks;
using BookingTravel.Application.DTOs.Booking;
using BookingTravel.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookingTravel.API.Controllers
{
    public class BookingController : BaseController
    {
        private readonly IBookingService _bookingService;

        public BookingController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        [HttpPost("create")]
        [Authorize] // Bắt buộc user phải đăng nhập (Bất kể Customer hay Admin)
        public async Task<IActionResult> CreateBooking([FromBody] CreateBookingRequest request)
        {
            try
            {
                // Tự động rút User ID từ trong cái chuỗi mã Token gửi lên API
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return ErrorResult("Thông tin đăng nhập không hợp lệ.", 401);
                }

                var response = await _bookingService.CreateBookingAsync(userId, request);
                return SuccessResult(response, "Khóa chỗ thành công.", 201);
            }
            catch (Exception ex)
            {
                // Bắt gọn lỗi Concurrency hoặc hết chỗ do Service bắn ra
                return ErrorResult(ex.Message, 400);
            }
        }

        [HttpGet("my")]
        [Authorize]
        public async Task<IActionResult> GetMyBookings()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return ErrorResult("Thông tin đăng nhập không hợp lệ.", 401);
                }

                var bookings = await _bookingService.GetMyBookingsAsync(userId);
                return SuccessResult(bookings, "Lấy danh sách 예약 thành công.");
            }
            catch (Exception ex)
            {
                return ErrorResult(ex.Message, 400);
            }
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetBookingById(int id)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return ErrorResult("Thông tin đăng nhập không hợp lệ.", 401);
                }

                var booking = await _bookingService.GetBookingByIdAsync(id, userId);
                if (booking == null) return ErrorResult("Không tìm thấy đơn hàng.", 404);

                return SuccessResult(booking, "Lấy chi tiết đơn hàng thành công.");
            }
            catch (Exception ex)
            {
                return ErrorResult(ex.Message, 400);
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllBookings()
        {
            try
            {
                var bookings = await _bookingService.GetAllBookingsAsync();
                return SuccessResult(bookings, "Lấy danh sách đơn hàng thành công.");
            }
            catch (Exception ex)
            {
                return ErrorResult(ex.Message, 400);
            }
        }
        [HttpPut("{id}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateBookingStatus(int id, [FromBody] UpdateBookingStatusRequest request)
        {
            try
            {
                var success = await _bookingService.UpdateBookingStatusAsync(id, request.Status);
                if (!success) return ErrorResult("Không tìm thấy đơn hàng cần cập nhật.", 404);
                return SuccessResult(null, "Cập nhật trạng thái thành công.");
            }
            catch (Exception ex)
            {
                return ErrorResult(ex.Message, 400);
            }
        }

        [HttpPut("{id}/cancel")]
        [Authorize]
        public async Task<IActionResult> CancelBooking(int id)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return ErrorResult("Thông tin đăng nhập không hợp lệ.", 401);
                }

                var success = await _bookingService.CancelBookingAsync(userId, id);
                if (!success) return ErrorResult("Không tìm thấy đơn hàng của bạn.", 404);

                return SuccessResult(null, "Đã hủy đơn hàng thành công.");
            }
            catch (Exception ex)
            {
                return ErrorResult(ex.Message, 400);
            }
        }
    }
}
