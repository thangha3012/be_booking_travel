using System.Collections.Generic;
using System.Threading.Tasks;
using BookingTravel.Application.DTOs.Category;

namespace BookingTravel.Application.Interfaces
{
    public interface ICategoryService
    {
        Task<IReadOnlyList<CategoryDto>> GetAllCategoriesAsync();
        Task<CategoryDto> CreateCategoryAsync(CreateCategoryRequest request);
        Task<bool> UpdateCategoryAsync(int id, UpdateCategoryRequest request);
        Task<bool> DeleteCategoryAsync(int id);
    }
}
