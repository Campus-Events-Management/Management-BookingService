using Microsoft.EntityFrameworkCore;
using EventManagement.BookingService.Models;

namespace EventManagement.BookingService.Data
{
    public class BookingDbContext : DbContext
    {
        public BookingDbContext(DbContextOptions<BookingDbContext> options) 
            : base(options)
        {
        }
        
        public DbSet<Booking> Bookings { get; set; } = null!;
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Configure Booking entity
            modelBuilder.Entity<Booking>()
                .HasIndex(b => new { b.EventId, b.UserId })
                .IsUnique();
                
            // Seeding will be handled in SeedData class
        }
    }
} 