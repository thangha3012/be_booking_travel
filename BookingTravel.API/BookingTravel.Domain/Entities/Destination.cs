using System;
using System.Collections.Generic;

namespace BookingTravel.Domain.Entities
{
    public class Destination : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Country { get; set; } = string.Empty;
        public string Province { get; set; } = string.Empty;
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string? CoverImageUrl { get; set; }

        public ICollection<Tour> Tours { get; set; } = new List<Tour>();
    }
}
