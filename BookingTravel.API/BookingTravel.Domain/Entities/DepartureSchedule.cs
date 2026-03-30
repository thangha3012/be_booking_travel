using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BookingTravel.Domain.Entities
{
    public class DepartureSchedule : BaseEntity
    {
        public int TourId { get; set; }
        public Tour Tour { get; set; } = null!;

        public DateTime DepartureDate { get; set; }
        public DateTime ReturnDate { get; set; }
        public int TotalSeats { get; set; }
        public int AvailableSeats { get; set; }
        public bool IsActive { get; set; } = true;

        // Dùng cho Optimistic Concurrency để tránh overbooking
        [Timestamp]
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();

        public ICollection<TourPricing> Pricings { get; set; } = new List<TourPricing>();
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}
