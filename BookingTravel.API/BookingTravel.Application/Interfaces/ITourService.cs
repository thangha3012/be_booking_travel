using System.Collections.Generic;
using System.Threading.Tasks;
using BookingTravel.Application.DTOs.Tours;

namespace BookingTravel.Application.Interfaces
{
    public interface ITourService
    {
        // Quản lý Tour cơ bản
        Task<PagedResult<TourDto>> GetAllToursAsync(
            string? keyword = null, 
            int? categoryId = null, 
            int? destinationId = null,
            int page = 1,
            int pageSize = 10,
            string? sortBy = null,
            decimal? minPrice = null,
            decimal? maxPrice = null,
            int? status = null);
        Task<TourDto?> GetTourByIdAsync(int id);
        Task<TourDto> CreateTourAsync(CreateTourRequest request);
        Task<bool> UpdateTourAsync(int id, UpdateTourRequest request);
        Task<bool> DeleteTourAsync(int id);

        // Quản lý Lịch trình (Schedule & Pricing)
        Task<DepartureScheduleDto> AddScheduleAsync(int tourId, AddScheduleRequest request);
        Task<bool> RemoveScheduleAsync(int scheduleId);
    }
}
