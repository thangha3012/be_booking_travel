using System;
using BookingTravel.Domain.Enums;

namespace BookingTravel.Domain.Entities
{
    public class Review : BaseEntity
    {
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public int TourId { get; set; }
        public Tour Tour { get; set; } = null!;

        public int BookingId { get; set; }
        public Booking Booking { get; set; } = null!;

        public int Rating { get; set; } = 5;
        public string? Title { get; set; }
        public string? Content { get; set; }
        public ReviewStatus Status { get; set; } = ReviewStatus.Pending;

        // Relation to MediaItem (Images attached to review)
    }
}
