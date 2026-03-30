using BookingTravel.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BookingTravel.Infrastructure.Data
{
    public class BookingTravelDbContext : DbContext
    {
        public BookingTravelDbContext(DbContextOptions<BookingTravelDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Destination> Destinations { get; set; }
        public DbSet<Tour> Tours { get; set; }
        public DbSet<DepartureSchedule> DepartureSchedules { get; set; }
        public DbSet<TourPricing> TourPricings { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Passenger> Passengers { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<BookingStatusHistory> BookingStatusHistories { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<MediaItem> MediaItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Entity Relationships and Constraints

            // User Email must be unique
            modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();

            // Tour - Category (1-N)
            modelBuilder.Entity<Tour>()
                .HasOne(t => t.Category)
                .WithMany(c => c.Tours)
                .HasForeignKey(t => t.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Tour - Destination (1-N)
            modelBuilder.Entity<Tour>()
                .HasOne(t => t.Destination)
                .WithMany(d => d.Tours)
                .HasForeignKey(t => t.DestinationId)
                .OnDelete(DeleteBehavior.Restrict);

            // DepartureSchedule - RowVersion cho khóa chống trùng lặp đặt chỗ
            modelBuilder.Entity<DepartureSchedule>()
                .Property(d => d.RowVersion)
                .IsRowVersion()
                .ValueGeneratedOnAddOrUpdate(); // Required for MySQL RowVersion in EF Pomelo

            // TourPricing decimal mapping
            modelBuilder.Entity<TourPricing>()
                .Property(tp => tp.Price)
                .HasColumnType("decimal(18,2)");

            // Payment amount mapping
            modelBuilder.Entity<Payment>()
                .Property(p => p.Amount)
                .HasColumnType("decimal(18,2)");

            // Booking amount mapping
            modelBuilder.Entity<Booking>()
                .Property(b => b.TotalAmount)
                .HasColumnType("decimal(18,2)");

            // Category self-referencing (1-N)
            modelBuilder.Entity<Category>()
                .HasOne(c => c.Parent)
                .WithMany(c => c.SubCategories)
                .HasForeignKey(c => c.ParentId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
