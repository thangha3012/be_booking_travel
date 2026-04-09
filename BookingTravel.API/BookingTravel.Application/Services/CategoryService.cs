using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BookingTravel.Application.DTOs.Category;
using BookingTravel.Application.Interfaces;
using BookingTravel.Domain.Entities;

namespace BookingTravel.Application.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly IRepository<Category> _categoryRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CategoryService(IRepository<Category> categoryRepository, IUnitOfWork unitOfWork)
        {
            _categoryRepository = categoryRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<IReadOnlyList<CategoryDto>> GetAllCategoriesAsync()
        {
            var categories = await _categoryRepository.GetAllAsync();
            return categories.Select(c => new CategoryDto
            {
                Id = c.Id,
                ParentId = c.ParentId,
                Name = c.Name,
                Slug = c.Slug,
                Description = c.Description,
                DisplayOrder = c.DisplayOrder,
                IsActive = c.IsActive
            }).ToList();
        }

        public async Task<CategoryDto> CreateCategoryAsync(CreateCategoryRequest request)
        {
            var category = new Category
            {
                Name = request.Name,
                Slug = request.Name.ToLower().Replace(" ", "-"), // Chuyển đổi tên thành Slug đơn giản
                Description = request.Description,
                ParentId = request.ParentId,
                DisplayOrder = request.DisplayOrder,
                IsActive = true
            };

            await _categoryRepository.AddAsync(category);
            await _unitOfWork.CompleteAsync();

            return new CategoryDto
            {
                Id = category.Id,
                ParentId = category.ParentId,
                Name = category.Name,
                Slug = category.Slug,
                Description = category.Description,
                DisplayOrder = category.DisplayOrder,
                IsActive = category.IsActive
            };
        }

        public async Task<bool> UpdateCategoryAsync(int id, UpdateCategoryRequest request)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null) return false;

            category.Name = request.Name;
            category.Slug = request.Name.ToLower().Replace(" ", "-");
            category.Description = request.Description;
            category.ParentId = request.ParentId;
            category.DisplayOrder = request.DisplayOrder;
            category.IsActive = request.IsActive;

            _categoryRepository.Update(category);
            var result = await _unitOfWork.CompleteAsync();
            return result > 0;
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null) return false;

            _categoryRepository.Delete(category);
            var result = await _unitOfWork.CompleteAsync();
            return result > 0;
        }
    }
}
