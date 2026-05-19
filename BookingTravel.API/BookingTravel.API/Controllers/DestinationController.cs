using System.Threading.Tasks;
using BookingTravel.Application.DTOs.Destinations;
using BookingTravel.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookingTravel.API.Controllers
{
    public class DestinationController : BaseController
    {
        private readonly IDestinationService _destinationService;

        public DestinationController(IDestinationService destinationService)
        {
            _destinationService = destinationService;
        }

        // Lấy danh sách toàn bộ các điểm đến du lịch
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _destinationService.GetAllDestinationsAsync();
            return SuccessResult(data, "Lấy danh sách điểm đến thành công.");
        }

        // Lấy chi tiết thông tin một điểm đến theo ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _destinationService.GetDestinationByIdAsync(id);
            if (result == null) return ErrorResult("Không tìm thấy điểm đến", 404);
            return SuccessResult(result, "Lấy thông tin điểm đến thành công.");
        }

        // Thêm một điểm đến mới vào hệ thống (Quyền Admin)
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateDestinationRequest request)
        {
            var result = await _destinationService.CreateDestinationAsync(request);
            return SuccessResult(result, "Tạo điểm đến mới thành công.", 201); // 201 Created
        }

        // Cập nhật thông tin điểm đến (Quyền Admin)
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateDestinationRequest request)
        {
            var success = await _destinationService.UpdateDestinationAsync(id, request);
            if (!success) return ErrorResult("Không tìm thấy điểm đến để cập nhật.", 404);
            
            return SuccessResult(null, "Cập nhật thành công.");
        }

        // Xóa điểm đến khỏi hệ thống (Quyền Admin)
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            // Trong thực tế, các hệ thống lớn thường áp dụng "Xoá mềm" (Soft Delete) - chỉ set cờ IsActive = false.
            // Đoạn code này là Xoá cứng để làm sạch CSDL theo mẫu chuẩn.
            var success = await _destinationService.DeleteDestinationAsync(id);
            if (!success) return ErrorResult("Không tìm thấy điểm đến để xóa.", 404);
            
            return SuccessResult(null, "Xóa thành công.");
        }
    }
}
