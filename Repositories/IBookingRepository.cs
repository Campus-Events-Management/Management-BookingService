using EventManagement.BookingService.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventManagement.BookingService.Repositories
{
    public interface IBookingRepository
    {
        Task<IEnumerable<Booking>> GetAllBookingsAsync();
        Task<IEnumerable<Booking>> GetBookingsByEventIdAsync(string eventId);
        Task<IEnumerable<Booking>> GetBookingsByUserIdAsync(string userId);
        Task<Booking?> GetBookingByIdAsync(int id);
        Task<Booking?> GetBookingByEventAndUserAsync(string eventId, string userId);
        Task<Booking> CreateBookingAsync(Booking booking);
        Task<bool> UpdateBookingAsync(Booking booking);
        Task<bool> DeleteBookingAsync(int id);
        Task<bool> SaveChangesAsync();
    }
} 