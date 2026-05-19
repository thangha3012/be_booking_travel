using System;
using System.Security.Claims;
using System.Threading.Tasks;
using BookingTravel.Application.Interfaces;
using BookingTravel.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookingTravel.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewController : ControllerBase
    {
        private readonly IReviewService _reviewService;

        public ReviewController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        // Lấy danh sách đánh giá của một Tour cụ thể
        [HttpGet("tour/{tourId}")]
        public async Task<IActionResult> GetByTourId(int tourId)
        {
            var reviews = await _reviewService.GetByTourIdAsync(tourId);
            return Ok(new { success = true, data = reviews });
        }

        // Lấy danh sách các đánh giá mới nhất trên hệ thống
        [HttpGet("latest")]
        public async Task<IActionResult> GetLatest([FromQuery] int count = 6)
        {
            var reviews = await _reviewService.GetLatestReviewsAsync(count);
            return Ok(new { success = true, data = reviews });
        }

        // Quản lý toàn bộ đánh giá (Dành cho Admin/Staff)
        [HttpGet("admin/all")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetAllForAdmin()
        {
            var reviews = await _reviewService.GetAllReviewsForAdminAsync();
            return Ok(new { success = true, data = reviews });
        }

        // Gửi đánh giá mới cho Tour (Yêu cầu đăng nhập)
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateReview([FromBody] CreateReviewDto request)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out int userId))
            {
                return Unauthorized();
            }

            // Optional: User can only review if they haven't already
            if (await _reviewService.HasUserReviewedTourAsync(userId, request.TourId))
            {
                return BadRequest(new { success = false, message = "Bạn đã đánh giá tour này rồi." });
            }

            var review = await _reviewService.CreateReviewAsync(userId, request.TourId, request.BookingId, request.Rating, request.Title, request.Content);
            return Ok(new { success = true, data = review, message = "Cảm ơn bạn đã đánh giá! Nhận xét của bạn đang chờ phê duyệt." });
        }

        // Duyệt hoặc ẩn đánh giá (Dành cho Admin/Staff)
        [HttpPut("{id}/status")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateReviewStatusDto request)
        {
            try
            {
                await _reviewService.UpdateReviewStatusAsync(id, request.Status);
                return Ok(new { success = true, message = "Cập nhật trạng thái thành công" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // Xóa vĩnh viễn một đánh giá (Yêu cầu quyền Admin)
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteReview(int id)
        {
            await _reviewService.DeleteReviewAsync(id);
            return Ok(new { success = true, message = "Xóa đánh giá thành công" });
        }
    }

    public class CreateReviewDto
    {
        public int TourId { get; set; }
        public int BookingId { get; set; }
        public int Rating { get; set; }
        public string? Title { get; set; }
        public string? Content { get; set; }
    }

    public class UpdateReviewStatusDto
    {
        public ReviewStatus Status { get; set; }
    }
}
