using System.Collections.Generic;
using System.Threading.Tasks;
using BookingTravel.Application.DTOs.Tours;

namespace BookingTravel.Application.Interfaces
{
    public interface ITourService
    {
        // Quản lý Tour cơ bản
        Task<IReadOnlyList<TourDto>> GetAllToursAsync();
        Task<TourDto?> GetTourByIdAsync(int id);
        Task<TourDto> CreateTourAsync(CreateTourRequest request);
        Task<bool> UpdateTourAsync(int id, UpdateTourRequest request);
        Task<bool> DeleteTourAsync(int id);

        // Quản lý Lịch trình (Schedule & Pricing)
        Task<DepartureScheduleDto> AddScheduleAsync(int tourId, AddScheduleRequest request);
        Task<bool> RemoveScheduleAsync(int scheduleId);
    }
}
