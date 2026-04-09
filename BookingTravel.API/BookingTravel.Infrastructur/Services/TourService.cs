using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BookingTravel.Application.DTOs.Tours;
using BookingTravel.Application.Interfaces;
using BookingTravel.Domain.Entities;
using BookingTravel.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using BookingTravel.Infrastructure.Data;

namespace BookingTravel.Infrastructure.Services
{
    public class TourService : ITourService
    {
        private readonly BookingTravelDbContext _context;
        private readonly IUnitOfWork _unitOfWork;

        // Lưu ý: Do chức năng load Navigation Property (Includes như Category, Schedules) khá phức tạp,
        // chúng ta sẽ inject DbContext thẳng vào đây để xài Linq Include cho tiện, 
        // kết hợp với IUnitOfWork để quản lý Transaction.
        public TourService(BookingTravelDbContext context, IUnitOfWork unitOfWork)
        {
            _context = context;
            _unitOfWork = unitOfWork;
        }

        public async Task<IReadOnlyList<TourDto>> GetAllToursAsync()
        {
            var tours = await _context.Tours
                .Include(t => t.Category)
                .Include(t => t.Destination)
                .ToListAsync();

            return tours.Select(MapToTourDto).ToList();
        }

        public async Task<TourDto?> GetTourByIdAsync(int id)
        {
            var tour = await _context.Tours
                .Include(t => t.Category)
                .Include(t => t.Destination)
                .Include(t => t.DepartureSchedules)
                    .ThenInclude(ds => ds.Pricings)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (tour == null) return null;
            
            return MapToTourDto(tour);
        }

        public async Task<TourDto> CreateTourAsync(CreateTourRequest request)
        {
            var tour = new Tour
            {
                CategoryId = request.CategoryId,
                DestinationId = request.DestinationId,
                Title = request.Title,
                Slug = request.Title.ToLower().Replace(" ", "-").Replace("đ", "d"),
                Description = request.Description,
                Highlights = request.Highlights,
                Itinerary = request.Itinerary,
                Policies = request.Policies,
                Status = TourStatus.Draft
            };

            await _context.Tours.AddAsync(tour);
            await _unitOfWork.CompleteAsync();

            return await GetTourByIdAsync(tour.Id) ?? throw new Exception("Vừa thêm xong nhưng không thấy trong DB?");
        }

        public async Task<bool> UpdateTourAsync(int id, UpdateTourRequest request)
        {
            var tour = await _context.Tours.FindAsync(id);
            if (tour == null) return false;

            tour.CategoryId = request.CategoryId;
            tour.DestinationId = request.DestinationId;
            tour.Title = request.Title;
            tour.Slug = request.Title.ToLower().Replace(" ", "-").Replace("đ", "d");
            tour.Description = request.Description;
            tour.Highlights = request.Highlights;
            tour.Itinerary = request.Itinerary;
            tour.Policies = request.Policies;
            tour.Status = request.Status;

            _context.Tours.Update(tour);
            await _unitOfWork.CompleteAsync();

            return true;
        }

        public async Task<bool> DeleteTourAsync(int id)
        {
            var tour = await _context.Tours.Include(t => t.DepartureSchedules).FirstOrDefaultAsync(t => t.Id == id);
            if (tour == null) return false;

            if (tour.DepartureSchedules.Any())
            {
                throw new Exception("Không thể xóa Tour vì đã có Lịch khởi hành. Hãy xóa lịch khởi hành trước, hoặc ẩn Tour này đi.");
            }

            _context.Tours.Remove(tour);
            await _unitOfWork.CompleteAsync();

            return true;
        }

        public async Task<DepartureScheduleDto> AddScheduleAsync(int tourId, AddScheduleRequest request)
        {
            var tour = await _context.Tours.FindAsync(tourId);
            if (tour == null) throw new Exception("Tour không tồn tại!");

            var schedule = new DepartureSchedule
            {
                TourId = tourId,
                DepartureDate = request.DepartureDate,
                ReturnDate = request.ReturnDate,
                TotalSeats = request.TotalSeats,
                AvailableSeats = request.TotalSeats, // Ban đầu số ghế rảnh = tổng ghế
                IsActive = true,
                Pricings = request.Pricings.Select(p => new TourPricing
                {
                    PassengerType = p.PassengerType,
                    Price = p.Price
                }).ToList()
            };

            await _context.DepartureSchedules.AddAsync(schedule);
            await _unitOfWork.CompleteAsync();

            return new DepartureScheduleDto
            {
                Id = schedule.Id,
                DepartureDate = schedule.DepartureDate,
                ReturnDate = schedule.ReturnDate,
                TotalSeats = schedule.TotalSeats,
                AvailableSeats = schedule.AvailableSeats,
                IsActive = schedule.IsActive,
                Pricings = schedule.Pricings.Select(p => new TourPricingDto { PassengerType = p.PassengerType, Price = p.Price }).ToList()
            };
        }

        public async Task<bool> RemoveScheduleAsync(int scheduleId)
        {
            var schedule = await _context.DepartureSchedules.Include(s => s.Bookings).FirstOrDefaultAsync(s => s.Id == scheduleId);
            if (schedule == null) return false;

            if (schedule.Bookings.Any())
            {
                throw new Exception("Lịch trình này đã có khách hàng Booking, không thể xóa!");
            }

            _context.DepartureSchedules.Remove(schedule);
            await _unitOfWork.CompleteAsync();

            return true;
        }

        private TourDto MapToTourDto(Tour tour)
        {
            var dto = new TourDto
            {
                Id = tour.Id,
                CategoryId = tour.CategoryId,
                CategoryName = tour.Category?.Name ?? "Unknown",
                DestinationId = tour.DestinationId,
                DestinationName = tour.Destination?.Name ?? "Unknown",
                Title = tour.Title,
                Slug = tour.Slug,
                Description = tour.Description,
                Highlights = tour.Highlights,
                Itinerary = tour.Itinerary,
                Policies = tour.Policies,
                Status = tour.Status
            };

            if (tour.DepartureSchedules != null && tour.DepartureSchedules.Any())
            {
                dto.Schedules = tour.DepartureSchedules.Select(ds => new DepartureScheduleDto
                {
                    Id = ds.Id,
                    DepartureDate = ds.DepartureDate,
                    ReturnDate = ds.ReturnDate,
                    TotalSeats = ds.TotalSeats,
                    AvailableSeats = ds.AvailableSeats,
                    IsActive = ds.IsActive,
                    Pricings = ds.Pricings != null ? ds.Pricings.Select(p => new TourPricingDto
                    {
                        PassengerType = p.PassengerType,
                        Price = p.Price
                    }).ToList() : new List<TourPricingDto>()
                }).ToList();
            }

            return dto;
        }
    }
}
