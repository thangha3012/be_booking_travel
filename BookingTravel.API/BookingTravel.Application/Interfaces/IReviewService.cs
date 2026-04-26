using System.Collections.Generic;
using System.Threading.Tasks;
using BookingTravel.Domain.Entities;
using BookingTravel.Domain.Enums;

namespace BookingTravel.Application.Interfaces
{
    public interface IReviewService
    {
        Task<IReadOnlyList<Review>> GetByTourIdAsync(int tourId);
        Task<IReadOnlyList<Review>> GetAllReviewsForAdminAsync();
        Task<Review?> GetByIdAsync(int id);
        Task<Review> CreateReviewAsync(int userId, int tourId, int bookingId, int rating, string? title, string? content);
        Task UpdateReviewStatusAsync(int id, ReviewStatus status);
        Task DeleteReviewAsync(int id);
        Task<bool> HasUserReviewedTourAsync(int userId, int tourId);
    }
}
