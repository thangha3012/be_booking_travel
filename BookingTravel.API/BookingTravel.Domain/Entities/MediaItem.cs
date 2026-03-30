using System;
using BookingTravel.Domain.Enums;

namespace BookingTravel.Domain.Entities
{
    public class MediaItem : BaseEntity
    {
        public int RefId { get; set; } // Can be TourId or ReviewId
        public string RefType { get; set; } = "Tour"; // "Tour" or "Review"
        
        public string Url { get; set; } = string.Empty;
        public string Type { get; set; } = "Image"; // "Image" or "Video"
    }
}
