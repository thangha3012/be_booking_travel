using System;
using System.Threading.Tasks;
using BookingTravel.API.Controllers;
using BookingTravel.Application.DTOs.Tours;
using BookingTravel.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookingTravel.API.Controllers
{
    public class TourController : BaseController
    {
        private readonly ITourService _tourService;

        public TourController(ITourService tourService)
        {
            _tourService = tourService;
        }

        // Khách hàng vãng lai có thể xem danh sách Tour thoải mái
        [HttpGet]
        public async Task<IActionResult> GetAllTours([FromQuery] string? keyword = null, [FromQuery] int? categoryId = null, [FromQuery] int? destinationId = null)
        {
            var tours = await _tourService.GetAllToursAsync(keyword, categoryId, destinationId);
            return SuccessResult(tours, "Lấy danh sách Tour thành công.");
        }

        // Khách lúc bấm vào Chi tiết 1 Tour
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTourById(int id)
        {
            var tour = await _tourService.GetTourByIdAsync(id);
            if (tour == null) return ErrorResult("Không tìm thấy Tour này.", 404);
            return SuccessResult(tour, "Lấy chi tiết Tour thành công.");
        }

        // === CÁC API DƯỚI ĐÂY CHỈ ADMIN MỚI ĐƯỢC PHÉP ĐỤNG VÀO (Tạo, sửa, xóa, gắn lịch) ===

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateTour([FromBody] CreateTourRequest request)
        {
            var newTour = await _tourService.CreateTourAsync(request);
            return SuccessResult(newTour, "Tạo hệ thống Tour nền tảng thành công.", 201);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateTour(int id, [FromBody] UpdateTourRequest request)
        {
            var success = await _tourService.UpdateTourAsync(id, request);
            if (!success) return ErrorResult("Không tìm thấy Tour.");
            return SuccessResult(null, "Cập nhật Tour thành công.");
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteTour(int id)
        {
            try
            {
                var success = await _tourService.DeleteTourAsync(id);
                if (!success) return ErrorResult("Không tìm thấy Tour.");
                return SuccessResult(null, "Xóa Tour thành công.");
            }
            catch (Exception ex)
            {
                return ErrorResult(ex.Message, 400); // Lỗi ràng buộc (ví dụ: đang có lịch trình không cho xóa)
            }
        }

        [HttpPost("{id}/schedules")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddSchedule(int id, [FromBody] AddScheduleRequest request)
        {
            try
            {
                var newSchedule = await _tourService.AddScheduleAsync(id, request);
                return SuccessResult(newSchedule, "Gắn lịch trình và bảng giá vào Tour thành công.", 201);
            }
            catch (Exception ex)
            {
                return ErrorResult(ex.Message, 400);
            }
        }

        [HttpDelete("schedules/{scheduleId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RemoveSchedule(int scheduleId)
        {
            try
            {
                var success = await _tourService.RemoveScheduleAsync(scheduleId);
                if (!success) return ErrorResult("Không tìm thấy lịch trình cần xóa.");
                return SuccessResult(null, "Gỡ lịch trình thành công.");
            }
            catch (Exception ex)
            {
                return ErrorResult(ex.Message, 400); // Lỗi ràng buộc (đã có khách book)
            }
        }
    }
}
