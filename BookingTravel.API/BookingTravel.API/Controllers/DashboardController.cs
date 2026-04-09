using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BookingTravel.Infrastructure.Data;
using BookingTravel.Domain.Enums;
using System.Linq;

namespace BookingTravel.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class DashboardController : BaseController
    {
        private readonly BookingTravelDbContext _context;

        public DashboardController(BookingTravelDbContext context)
        {
            _context = context;
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetDashboardStats()
        {
            var totalTours = await _context.Tours.CountAsync();
            var totalUsers = await _context.Users.CountAsync();
            
            // Tính số lượng pending booking
            var pendingBookings = await _context.Bookings.CountAsync(b => b.Status == BookingStatus.Pending);

            // Tính tổng tiền từ các bookings đã Complete hoặc Confirmed
            var totalRevenue = await _context.Bookings
                .Where(b => b.Status == BookingStatus.Completed || b.Status == BookingStatus.Confirmed)
                .SumAsync(b => b.TotalAmount);

            return SuccessResult(new
            {
                TotalTours = totalTours,
                TotalUsers = totalUsers,
                PendingBookings = pendingBookings,
                TotalRevenue = totalRevenue
            }, "Thống kê Dashboard");
        }
    }
}
