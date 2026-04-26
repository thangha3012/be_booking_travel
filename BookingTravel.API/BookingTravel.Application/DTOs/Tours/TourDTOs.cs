using System;
using System.Collections.Generic;
using BookingTravel.Domain.Enums;

namespace BookingTravel.Application.DTOs.Tours
{
    // ====== DTOs cho Xem dữ liệu (Responses) ======
    public class TourDto
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int DestinationId { get; set; }
        public string DestinationName { get; set; } = string.Empty;
        
        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Highlights { get; set; }
        public string? Itinerary { get; set; }
        public string? Policies { get; set; }
        public string? TourCode { get; set; }
        public string? Duration { get; set; }
        public decimal? BasePrice { get; set; }
        public string? DepartureLocation { get; set; }
        public string? Transport { get; set; }
        public TourStatus Status { get; set; }
        public string? ImageUrl { get; set; }
        public double Rating { get; set; }

        public List<DepartureScheduleDto> Schedules { get; set; } = new();
    }

    public class DepartureScheduleDto
    {
        public int Id { get; set; }
        public DateTime DepartureDate { get; set; }
        public DateTime ReturnDate { get; set; }
        public int TotalSeats { get; set; }
        public int AvailableSeats { get; set; }
        public bool IsActive { get; set; }
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
        public List<TourPricingDto> Pricings { get; set; } = new();
    }

    public class TourPricingDto
    {
        public PassengerType PassengerType { get; set; }
        public decimal Price { get; set; }
    }


    // ====== DTOs cho Thêm/Sửa dữ liệu (Requests) ======
    public class CreateTourRequest
    {
        public int CategoryId { get; set; }
        public int DestinationId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Highlights { get; set; }
        public string? Itinerary { get; set; }
        public string? Policies { get; set; }
        public string? TourCode { get; set; }
        public string? Duration { get; set; }
        public decimal? BasePrice { get; set; }
        public string? DepartureLocation { get; set; }
        public string? Transport { get; set; }
    }

    public class UpdateTourRequest : CreateTourRequest
    {
        public TourStatus Status { get; set; }
    }

    public class AddScheduleRequest
    {
        public DateTime DepartureDate { get; set; }
        public DateTime ReturnDate { get; set; }
        public int TotalSeats { get; set; }
        public List<TourPricingDto> Pricings { get; set; } = new();
    }

    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    }
}
