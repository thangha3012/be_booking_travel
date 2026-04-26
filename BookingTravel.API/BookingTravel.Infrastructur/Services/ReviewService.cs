using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BookingTravel.Application.Interfaces;
using BookingTravel.Domain.Entities;
using BookingTravel.Domain.Enums;
using BookingTravel.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BookingTravel.Infrastructure.Services
{
    public class ReviewService : IReviewService
    {
        private readonly BookingTravelDbContext _context;

        public ReviewService(BookingTravelDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<Review>> GetByTourIdAsync(int tourId)
        {
            // For customers: Only show Approved reviews
            return await _context.Reviews
                .Include(r => r.User)
                .Where(r => r.TourId == tourId && r.Status == ReviewStatus.Approved)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<Review>> GetAllReviewsForAdminAsync()
        {
            // For admin: Show all reviews with any statuses
            return await _context.Reviews
                .Include(r => r.User)
                .Include(r => r.Tour)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<Review?> GetByIdAsync(int id)
        {
            return await _context.Reviews
                .Include(r => r.User)
                .Include(r => r.Tour)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<Review> CreateReviewAsync(int userId, int tourId, int bookingId, int rating, string? title, string? content)
        {
            var review = new Review
            {
                UserId = userId,
                TourId = tourId,
                BookingId = bookingId,
                Rating = rating,
                Title = title,
                Content = content,
                Status = ReviewStatus.Pending // Require admin approval
            };

            await _context.Reviews.AddAsync(review);
            await _context.SaveChangesAsync();

            return review;
        }

        public async Task UpdateReviewStatusAsync(int id, ReviewStatus status)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review == null)
            {
                throw new Exception("Không tìm thấy đánh giá nào với ID cung cấp.");
            }

            review.Status = status;
            await _context.SaveChangesAsync();

            // Optionally, update the Tour's average rating if it becomes Approved, or Recalculate it
            if (status == ReviewStatus.Approved)
            {
                await RecalculateTourRatingAsync(review.TourId);
            }
            else 
            {
                await RecalculateTourRatingAsync(review.TourId);
            }
        }

        public async Task DeleteReviewAsync(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review != null)
            {
                int tourId = review.TourId;
                _context.Reviews.Remove(review);
                await _context.SaveChangesAsync();
                await RecalculateTourRatingAsync(tourId);
            }
        }

        public async Task<bool> HasUserReviewedTourAsync(int userId, int tourId)
        {
            return await _context.Reviews.AnyAsync(r => r.UserId == userId && r.TourId == tourId);
        }

        private async Task RecalculateTourRatingAsync(int tourId)
        {
            var tour = await _context.Tours.FindAsync(tourId);
            if (tour == null) return;

            var approvedReviews = await _context.Reviews
                .Where(r => r.TourId == tourId && r.Status == ReviewStatus.Approved)
                .ToListAsync();

            if (approvedReviews.Any())
            {
                tour.Rating = approvedReviews.Average(r => r.Rating);
            }
            else
            {
                tour.Rating = 0;
            }

            await _context.SaveChangesAsync();
        }
    }
}
