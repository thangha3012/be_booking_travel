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
        public string? TourCode { get; set; }
        public string? Duration { get; set; }
        public decimal? BasePrice { get; set; }
        public string? DepartureLocation { get; set; }
        public string? Transport { get; set; }
        public TourStatus Status { get; set; } = TourStatus.Draft;
        public string? ImageUrl { get; set; }
        public double Rating { get; set; } = 0;

        public ICollection<DepartureSchedule> DepartureSchedules { get; set; } = new List<DepartureSchedule>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
    }
}
