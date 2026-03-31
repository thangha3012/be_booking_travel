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

        // Ai cũng có thể xem danh mục (Không cần phân quyền)
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            return SuccessResult(categories, "Lấy danh sách danh mục thành công.");
        }

        // CHỈ CÓ ADMIN mới được quyền tạo danh mục
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
    }
}
