using System;
using BookingTravel.Domain.Enums;

namespace BookingTravel.Domain.Entities
{
    public class Passenger : BaseEntity
    {
        public int BookingId { get; set; }
        public Booking Booking { get; set; } = null!;

        public string FullName { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public DateTime? DateOfBirth { get; set; }
        public string? IdDocument { get; set; }
        public PassengerType Type { get; set; } = PassengerType.Adult;
    }
}
