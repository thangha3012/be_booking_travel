using System;
using System.Collections.Generic;

namespace BookingTravel.Domain.Entities
{
    public class Category : BaseEntity
    {
        public int? ParentId { get; set; }
        public Category? Parent { get; set; }

        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int DisplayOrder { get; set; } = 0;
        public bool IsActive { get; set; } = true;

        public ICollection<Category> SubCategories { get; set; } = new List<Category>();
        public ICollection<Tour> Tours { get; set; } = new List<Tour>();
    }
}
