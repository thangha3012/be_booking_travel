using System.Threading.Tasks;
using BookingTravel.Application.DTOs.Booking;

namespace BookingTravel.Application.Interfaces
{
    public interface IBookingService
    {
        Task<BookingResponseDto> CreateBookingAsync(int userId, CreateBookingRequest request);
        Task<IEnumerable<MyBookingDto>> GetMyBookingsAsync(int userId);
        Task<IEnumerable<AdminBookingDto>> GetAllBookingsAsync();
        Task<bool> UpdateBookingStatusAsync(int bookingId, BookingTravel.Domain.Enums.BookingStatus status);
    }
}
