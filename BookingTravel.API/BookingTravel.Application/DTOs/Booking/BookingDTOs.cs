using System;
using System.Collections.Generic;
using BookingTravel.Domain.Enums;

namespace BookingTravel.Application.DTOs.Booking
{
    // DTO cho Hành khách
    public class PassengerDto
    {
        public string FullName { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public DateTime? DateOfBirth { get; set; }
        public string? IdDocument { get; set; }
        public PassengerType Type { get; set; }
    }

    // Yêu cầu Tạo Đơn Hàng (Của Khách Hàng)
    public class CreateBookingRequest
    {
        public int TourId { get; set; }
        public int DepartureScheduleId { get; set; }

        public string ContactName { get; set; } = string.Empty;
        public string ContactPhone { get; set; } = string.Empty;
        public string ContactEmail { get; set; } = string.Empty;
        public string? Notes { get; set; }

        // RowVersion từ Lịch trình truyền xuống để bắt ngoại lệ Concurrency
        public byte[]? ScheduleRowVersion { get; set; }

        // Danh sách khách đi tour
        public List<PassengerDto> Passengers { get; set; } = new List<PassengerDto>();
    }

    // Kết quả trả về sau khi đặt thành công
    public class BookingResponseDto
    {
        public int BookingId { get; set; }
        public int TourId { get; set; }
        public string TourName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public BookingStatus Status { get; set; }
        public DateTime UnlockTime { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class MyBookingDto
    {
        public int Id { get; set; }
        public int TourId { get; set; }
        public string TourName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public BookingStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime DepartureDate { get; set; }
        public int NumberOfPassengers { get; set; }
    }

    public class AdminBookingDto : MyBookingDto
    {
        public string ContactName { get; set; } = string.Empty;
        public string ContactPhone { get; set; } = string.Empty;
        public string ContactEmail { get; set; } = string.Empty;
        public int UserId { get; set; }
    }

    public class UpdateBookingStatusRequest
    {
        public BookingStatus Status { get; set; }
    }
}
