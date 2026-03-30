using System;
using System.Collections.Generic;
using BookingTravel.Domain.Enums;

namespace BookingTravel.Domain.Entities
{
    public class Booking : BaseEntity
    {
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public int TourId { get; set; }
        public Tour Tour { get; set; } = null!;

        public int DepartureScheduleId { get; set; }
        public DepartureSchedule DepartureSchedule { get; set; } = null!;

        public decimal TotalAmount { get; set; }
        public BookingStatus Status { get; set; } = BookingStatus.Pending;

        public string ContactName { get; set; } = string.Empty;
        public string ContactPhone { get; set; } = string.Empty;
        public string ContactEmail { get; set; } = string.Empty;
        public string? Notes { get; set; }

        public DateTime? UnlockTime { get; set; } // Dùng để xử lý Inventory Lock (15-30 phút)

        public ICollection<Passenger> Passengers { get; set; } = new List<Passenger>();
        public ICollection<BookingStatusHistory> StatusHistories { get; set; } = new List<BookingStatusHistory>();
    }
}
