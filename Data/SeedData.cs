using EventManagement.BookingService.Models;
using System;

namespace EventManagement.BookingService.Data
{
    public static class SeedData
    {
        public static void Initialize(BookingDbContext context)
        {
            // Check if there are any bookings already
            if (context.Bookings.Any())
            {
                return; // DB has already been seeded
            }
            
            // Add sample bookings
            var bookings = new Booking[]
            {
                new Booking
                {
                    EventId = "e1b5d9d7-8cd4-4836-b978-15c4a513964a",
                    UserId = "user1",
                    BookingDate = DateTime.Now.AddDays(5),
                    CreatedAt = DateTime.Now.AddDays(-2),
                    Status = "Confirmed"
                },
                new Booking
                {
                    EventId = "e1b5d9d7-8cd4-4836-b978-15c4a513964a",
                    UserId = "user2",
                    BookingDate = DateTime.Now.AddDays(5),
                    CreatedAt = DateTime.Now.AddDays(-1),
                    Status = "Confirmed"
                },
                new Booking
                {
                    EventId = "a7d86672-45d8-4d42-8ae9-6e8438b81061",
                    UserId = "user1",
                    BookingDate = DateTime.Now.AddDays(10),
                    CreatedAt = DateTime.Now.AddDays(-3),
                    Status = "Confirmed"
                },
                new Booking
                {
                    EventId = "c1f9b4d7-1f5c-4e7b-9d8a-2b5f3c7d8e9a",
                    UserId = "user3",
                    BookingDate = DateTime.Now.AddDays(7),
                    CreatedAt = DateTime.Now.AddDays(-5),
                    Status = "Cancelled",
                    CancellationReason = "Schedule conflict"
                }
            };
            
            context.Bookings.AddRange(bookings);
            context.SaveChanges();
        }
    }
} 