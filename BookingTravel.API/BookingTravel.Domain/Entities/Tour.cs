using System;
using System.Collections.Generic;
using BookingTravel.Domain.Enums;

namespace BookingTravel.Domain.Entities
{
    public class Tour : BaseEntity
    {
        public int CategoryId { get; set; }
        public Category Category { get; set; } = null!;

        public int DestinationId { get; set; }
        public Destination Destination { get; set; } = null!;

        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Highlights { get; set; }
        public string? Itinerary { get; set; } // Can store JSON
        public string? Policies { get; set; } // Can store JSON
        public TourStatus Status { get; set; } = TourStatus.Draft;

        public ICollection<DepartureSchedule> DepartureSchedules { get; set; } = new List<DepartureSchedule>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
    }
}
