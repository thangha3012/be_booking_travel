using System.Threading.Tasks;
using System.Collections.Generic;
using BookingTravel.Application.DTOs.Booking;

namespace BookingTravel.Application.Interfaces
{
    public interface IBookingService
    {
        Task<BookingResponseDto> CreateBookingAsync(int userId, CreateBookingRequest request);
        Task<IEnumerable<MyBookingDto>> GetMyBookingsAsync(int userId);
        Task<AdminBookingDto> GetBookingByIdAsync(int bookingId, int userId);
        Task<IEnumerable<AdminBookingDto>> GetAllBookingsAsync();
        Task<bool> UpdateBookingStatusAsync(int bookingId, BookingTravel.Domain.Enums.BookingStatus status);
        Task<bool> CancelBookingAsync(int userId, int bookingId);
        Task<IEnumerable<TourParticipantDto>> GetTourParticipantsAsync();
    }
}
