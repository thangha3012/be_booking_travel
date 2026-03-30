using System;
using BookingTravel.Domain.Enums;

namespace BookingTravel.Domain.Entities
{
    public class BookingStatusHistory : BaseEntity
    {
        public int BookingId { get; set; }
        public Booking Booking { get; set; } = null!;

        public BookingStatus Status { get; set; }
        public string? Note { get; set; }
        public int? ChangedById { get; set; }
        public User? ChangedBy { get; set; }
    }
}
