using System;
using System.Threading.Tasks;
using BookingTravel.API.Controllers;
using BookingTravel.Application.DTOs.Category;
using BookingTravel.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookingTravel.API.Controllers
{
    public class CategoryController : BaseController
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        // Lấy danh sách toàn bộ danh mục tour
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            return SuccessResult(categories, "Lấy danh sách danh mục thành công.");
        }

        // Tạo mới một danh mục (Yêu cầu quyền Admin)
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateCategoryRequest request)
        {
            try
            {
                var newCategory = await _categoryService.CreateCategoryAsync(request);
                return SuccessResult(newCategory, "Tạo danh mục thành công.");
            }
            catch (Exception ex)
            {
                return ErrorResult(ex.Message);
            }
        }

        // Cập nhật thông tin danh mục (Yêu cầu quyền Admin)
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCategoryRequest request)
        {
            var success = await _categoryService.UpdateCategoryAsync(id, request);
            if (!success) return ErrorResult("Không tìm thấy danh mục hoặc cập nhật thất bại.");
            return SuccessResult(true, "Cập nhật danh mục thành công.");
        }

        // Xóa danh mục khỏi hệ thống (Yêu cầu quyền Admin)
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _categoryService.DeleteCategoryAsync(id);
            if (!success) return ErrorResult("Không tìm thấy danh mục hoặc xóa thất bại.");
            return SuccessResult(true, "Xóa danh mục thành công.");
        }
    }
}
