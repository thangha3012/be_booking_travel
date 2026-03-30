using System;
using BookingTravel.Domain.Enums;

namespace BookingTravel.Domain.Entities
{
    public class TourPricing : BaseEntity
    {
        public int DepartureScheduleId { get; set; }
        public DepartureSchedule DepartureSchedule { get; set; } = null!;

        public PassengerType PassengerType { get; set; } // Người lớn, Trẻ em, Em bé
        public decimal Price { get; set; }
    }
}
