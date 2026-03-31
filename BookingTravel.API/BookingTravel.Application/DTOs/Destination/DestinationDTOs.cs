using System.Collections.Generic;

namespace BookingTravel.Application.DTOs.Destinations
{
    public class DestinationDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Country { get; set; } = string.Empty;
        public string Province { get; set; } = string.Empty;
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string? CoverImageUrl { get; set; }
    }

    public class CreateDestinationRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Country { get; set; } = string.Empty;
        public string Province { get; set; } = string.Empty;
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string? CoverImageUrl { get; set; }
    }

    public class UpdateDestinationRequest : CreateDestinationRequest
    {
    }
}
