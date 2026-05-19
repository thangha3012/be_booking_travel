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

        // Tìm kiếm và lấy danh sách Tour với các bộ lọc (Phân trang, Giá, Danh mục,...)
        [HttpGet]
        public async Task<IActionResult> GetAllTours(
            [FromQuery] string? keyword = null, 
            [FromQuery] int? categoryId = null, 
            [FromQuery] int? destinationId = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? sortBy = null,
            [FromQuery] decimal? minPrice = null,
            [FromQuery] decimal? maxPrice = null,
            [FromQuery] int? status = null)
        {
            var pagedResult = await _tourService.GetAllToursAsync(keyword, categoryId, destinationId, page, pageSize, sortBy, minPrice, maxPrice, status);
            return SuccessResult(pagedResult, "Lấy danh sách Tour thành công.");
        }

        // Lấy thông tin chi tiết một Tour theo ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTourById(int id)
        {
            var tour = await _tourService.GetTourByIdAsync(id);
            if (tour == null) return ErrorResult("Không tìm thấy Tour này.", 404);
            return SuccessResult(tour, "Lấy chi tiết Tour thành công.");
        }

        // Tạo mới Tour du lịch (Yêu cầu quyền Admin)
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateTour([FromBody] CreateTourRequest request)
        {
            var newTour = await _tourService.CreateTourAsync(request);
            return SuccessResult(newTour, "Tạo hệ thống Tour nền tảng thành công.", 201);
        }

        // Cập nhật thông tin Tour (Yêu cầu quyền Admin)
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateTour(int id, [FromBody] UpdateTourRequest request)
        {
            var success = await _tourService.UpdateTourAsync(id, request);
            if (!success) return ErrorResult("Không tìm thấy Tour.");
            return SuccessResult(null, "Cập nhật Tour thành công.");
        }

        // Xóa Tour khỏi hệ thống (Yêu cầu quyền Admin)
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

        // Thêm lịch khởi hành cho Tour (Yêu cầu quyền Admin)
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

        // Gỡ bỏ lịch khởi hành của Tour (Yêu cầu quyền Admin)
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
