using System;
using System.Linq;
using System.Threading.Tasks;
using BookingTravel.Application.DTOs.Booking;
using BookingTravel.Application.Interfaces;
using BookingTravel.Domain.Entities;
using BookingTravel.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using BookingTravel.Infrastructure.Data;

namespace BookingTravel.Infrastructure.Services
{
    public class BookingService : IBookingService
    {
        private readonly BookingTravelDbContext _context;

        public BookingService(BookingTravelDbContext context)
        {
            _context = context;
        }

        public async Task<BookingResponseDto> CreateBookingAsync(int userId, CreateBookingRequest request)
        {
            // 1. Kiểm tra Lịch trình (Schedule) có tồn tại không
            var schedule = await _context.DepartureSchedules
                .Include(s => s.Tour)
                .Include(s => s.Pricings)
                .FirstOrDefaultAsync(s => s.Id == request.DepartureScheduleId);

            if (schedule == null)
            {
                throw new Exception("Lịch trình không tồn tại hoặc đã bị xóa.");
            }

            if (schedule.TourId != request.TourId)
            {
                 throw new Exception("Lịch trình không thuộc về Tour này.");
            }

            if (request.Passengers == null || !request.Passengers.Any())
            {
                throw new Exception("Danh sách hành khách không được để trống.");
            }

            // 2. Tính toán Giá tiền theo danh sách khách
            decimal totalAmount = 0;
            foreach (var pass in request.Passengers)
            {
                var priceTag = schedule.Pricings.FirstOrDefault(p => p.PassengerType == pass.Type);
                if (priceTag == null)
                {
                    // Nếu dữ liệu Tour trong DB bị lủng (Admin tạo thiếu giá), thì lấy giá mặc định để Demo không bị đứt đoạn.
                    decimal fallbackPrice = pass.Type == PassengerType.Child ? 100m : (pass.Type == PassengerType.Infant ? 50m : 150m);
                    totalAmount += fallbackPrice;
                }
                else
                {
                    totalAmount += priceTag.Price;
                }
            }

            // 3. Kiểm tra ghế trống dựa trên số lượng khách
            int totalPassengers = request.Passengers.Count;
            if (schedule.AvailableSeats < totalPassengers)
            {
                throw new Exception($"Lịch trình này chỉ còn lại {schedule.AvailableSeats} ghế trống, không đủ cho {totalPassengers} hành khách.");
            }

            // 4. Trừ đi số ghế trống của Lịch trình
            schedule.AvailableSeats -= totalPassengers;

            // 5. Khởi tạo đối tượng Booking mới
            var booking = new Booking
            {
                UserId = userId,
                TourId = request.TourId,
                DepartureScheduleId = request.DepartureScheduleId,
                TotalAmount = totalAmount,
                Status = BookingStatus.Pending,
                ContactName = request.ContactName,
                ContactEmail = request.ContactEmail,
                ContactPhone = request.ContactPhone,
                Notes = request.Notes,
                // Cho phép khách giữ chỗ 15 phút trước khi bị hủy tự động
                UnlockTime = DateTime.UtcNow.AddMinutes(15), 
                Passengers = request.Passengers.Select(p => new Passenger
                {
                    FullName = p.FullName,
                    DateOfBirth = p.DateOfBirth,
                    Gender = p.Gender,
                    IdDocument = p.IdDocument,
                    Type = p.Type
                }).ToList()
            };

            await _context.Bookings.AddAsync(booking);

            // 6. Ghi log Lịch sử
            booking.StatusHistories.Add(new BookingStatusHistory
            {
                Status = BookingStatus.Pending,
                Note = "Khởi tạo đơn hàng (Giữ chỗ 15 phút chờ thanh toán)."
            });

            // 7. Gắn RowVersion cũ mà Frontend truyền xuống để kích hoạt Optimistic Concurrency
            if (request.ScheduleRowVersion != null && request.ScheduleRowVersion.Length > 0)
            {
                _context.Entry(schedule).OriginalValues["RowVersion"] = request.ScheduleRowVersion;
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                // Đây chính là điểm Tinh Túy của Optimistic Concurrency
                throw new Exception("Rất tiếc! Vừa có người khác nhanh tay đặt mất những chiếc vé cuối cùng của lịch trình này. Vui lòng tải lại trang và chọn lịch khác.");
            }

            return new BookingResponseDto
            {
                BookingId = booking.Id,
                TourId = booking.TourId,
                TourName = schedule.Tour.Title,
                TotalAmount = booking.TotalAmount,
                Status = booking.Status,
                UnlockTime = booking.UnlockTime ?? DateTime.UtcNow,
                Message = "Tạo đơn đặt Tour thành công. Vui lòng thanh toán trong vòng 15 phút."
            };
        }

        public async Task<System.Collections.Generic.IEnumerable<MyBookingDto>> GetMyBookingsAsync(int userId)
        {
            var bookings = await _context.Bookings
                .Include(b => b.Tour)
                .Include(b => b.DepartureSchedule)
                .Include(b => b.Passengers)
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.CreatedAt)
                .Select(b => new MyBookingDto
                {
                    Id = b.Id,
                    TourId = b.TourId,
                    TourName = b.Tour.Title,
                    TotalAmount = b.TotalAmount,
                    Status = b.Status,
                    CreatedAt = b.CreatedAt,
                    DepartureDate = b.DepartureSchedule.DepartureDate,
                    NumberOfPassengers = b.Passengers.Count
                })
                .ToListAsync();

            return bookings;
        }

        public async Task<AdminBookingDto> GetBookingByIdAsync(int bookingId, int userId)
        {
            var b = await _context.Bookings
                .Include(x => x.Tour)
                .Include(x => x.DepartureSchedule)
                .Include(x => x.Passengers)
                .FirstOrDefaultAsync(x => x.Id == bookingId && x.UserId == userId);

            if (b == null) return null;

            return new AdminBookingDto
            {
                Id = b.Id,
                UserId = b.UserId,
                TourId = b.TourId,
                TourName = b.Tour.Title,
                TotalAmount = b.TotalAmount,
                Status = b.Status,
                CreatedAt = b.CreatedAt,
                DepartureDate = b.DepartureSchedule.DepartureDate,
                NumberOfPassengers = b.Passengers.Count,
                ContactName = b.ContactName,
                ContactPhone = b.ContactPhone,
                ContactEmail = b.ContactEmail
            };
        }

        public async Task<System.Collections.Generic.IEnumerable<AdminBookingDto>> GetAllBookingsAsync()
        {
            var bookings = await _context.Bookings
                .Include(b => b.Tour)
                .Include(b => b.DepartureSchedule)
                .Include(b => b.Passengers)
                .OrderByDescending(b => b.CreatedAt)
                .Select(b => new AdminBookingDto
                {
                    Id = b.Id,
                    UserId = b.UserId,
                    TourId = b.TourId,
                    TourName = b.Tour.Title,
                    TotalAmount = b.TotalAmount,
                    Status = b.Status,
                    CreatedAt = b.CreatedAt,
                    DepartureDate = b.DepartureSchedule.DepartureDate,
                    NumberOfPassengers = b.Passengers.Count,
                    ContactName = b.ContactName,
                    ContactPhone = b.ContactPhone,
                    ContactEmail = b.ContactEmail
                })
                .ToListAsync();

            return bookings;
        }

        public async Task<bool> UpdateBookingStatusAsync(int bookingId, BookingStatus status)
        {
            var booking = await _context.Bookings
                .Include(b => b.DepartureSchedule)
                .Include(b => b.Passengers)
                .FirstOrDefaultAsync(b => b.Id == bookingId);

            if (booking == null) return false;

            if (booking.Status == status) return true; // No change

            // Nếu hủy đơn hàng, trả lại số lượng ghế trống
            if (status == BookingStatus.Cancelled && booking.Status != BookingStatus.Cancelled)
            {
                if (booking.DepartureSchedule != null)
                {
                    booking.DepartureSchedule.AvailableSeats += booking.Passengers.Count;
                }
            }

            // Nếu chuyển từ Hủy sang trạng thái khác (Admin khôi phục đơn), cần trừ lại ghế.
            // Để an toàn, phải kiểm tra xem có đủ ghế không:
            if (booking.Status == BookingStatus.Cancelled && status != BookingStatus.Cancelled)
            {
                if (booking.DepartureSchedule != null)
                {
                    if (booking.DepartureSchedule.AvailableSeats < booking.Passengers.Count)
                    {
                        throw new Exception("Không đủ số lượng chỗ trống để khôi phục lại đơn hàng này.");
                    }
                    booking.DepartureSchedule.AvailableSeats -= booking.Passengers.Count;
                }
            }

            booking.Status = status;

            booking.StatusHistories.Add(new BookingStatusHistory
            {
                Status = status,
                Note = "Admin thay đổi trạng thái thành " + status.ToString()
            });

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CancelBookingAsync(int userId, int bookingId)
        {
            var booking = await _context.Bookings
                .Include(b => b.DepartureSchedule)
                .Include(b => b.Passengers)
                .FirstOrDefaultAsync(b => b.Id == bookingId && b.UserId == userId);

            if (booking == null) return false;

            // Chỉ cho phép khách hủy khi đơn hàng chưa được xác nhận hoặc đã hoàn thành
            if (booking.Status != BookingStatus.Pending && booking.Status != BookingStatus.AwaitingPayment)
            {
                throw new Exception("Không thể hủy đơn hàng ở trạng thái hiện tại. Vui lòng liên hệ hỗ trợ.");
            }

            // Hoàn trả ghế trống
            if (booking.DepartureSchedule != null)
            {
                booking.DepartureSchedule.AvailableSeats += booking.Passengers.Count;
            }

            booking.Status = BookingStatus.Cancelled;
            booking.StatusHistories.Add(new BookingStatusHistory
            {
                Status = BookingStatus.Cancelled,
                Note = "Khách hàng tự hủy đơn hàng."
            });

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
