using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BookingTravel.Domain.Enums;
using BookingTravel.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BookingTravel.Infrastructure.Workers
{
    // BackgroundService bản chất là 1 Singleton Worker chạy ngầm theo vòng đời của ứng dụng.
    public class BookingCleanupService : BackgroundService
    {
        private readonly ILogger<BookingCleanupService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        public BookingCleanupService(ILogger<BookingCleanupService> logger, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            // Vì DbContext là dịch vụ "Scoped" (mỗi request 1 cái), mà Background Job là hàng "Singleton" (tồn tại mãi mãi)
            // Nên chúng ta không thể nhồi trực tiếp DbContext vào đây được, mà phải đẻ ngầm qua ScopeFactory.
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("🚀 [Robot Booking Cleanup] Đã khởi động! Sẽ quét rác tự động 1 phút/lần.");

            // Vòng lặp vĩnh cửu chừng nào ứng dụng còn chạy
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CleanupExpiredBookingsAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ [Robot Booking Cleanup] Chết dọc đường khi dọn rác.");
                }

                // Nghỉ ngơi 1 phút (60 giây) trước khi quét lượt tiếp theo
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }

        private async Task CleanupExpiredBookingsAsync()
        {
            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<BookingTravelDbContext>();

            // Tìm tất cả các Booking đang cắm cờ Pending nhưng cái thời hạn UnlockTime đã qua 
            var expiredBookings = await dbContext.Bookings
                .Include(b => b.DepartureSchedule)
                .Include(b => b.Passengers)
                .Where(b => b.Status == BookingStatus.Pending && b.UnlockTime.HasValue && b.UnlockTime.Value <= DateTime.UtcNow)
                .ToListAsync();

            if (!expiredBookings.Any()) return;

            foreach (var booking in expiredBookings)
            {
                // 1. Phạt thẻ đỏ: Status = Cancelled
                booking.Status = BookingStatus.Cancelled;

                // 2. Trả lại ghế sạch để cho khách khác lên mua
                booking.DepartureSchedule.AvailableSeats += booking.Passengers.Count;

                // 3. Ghi vết lịch sử
                booking.StatusHistories.Add(new BookingTravel.Domain.Entities.BookingStatusHistory
                {
                    Status = BookingStatus.Cancelled,
                    Note = "Hệ thống tự động hủy booking do khách không thanh toán quá 15 phút."
                });

                _logger.LogInformation($"♻️ [Giải phóng] Đã hủy Booking #{booking.Id} và thối lại {booking.Passengers.Count} ghế cho Tour!");
            }

            // Lưu hết lại 1 lượt
            await dbContext.SaveChangesAsync();
        }
    }
}
