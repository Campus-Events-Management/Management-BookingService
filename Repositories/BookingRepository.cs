using EventManagement.BookingService.Data;
using EventManagement.BookingService.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EventManagement.BookingService.Repositories
{
    public class BookingRepository : IBookingRepository
    {
        private readonly BookingDbContext _context;
        
        public BookingRepository(BookingDbContext context)
        {
            _context = context;
        }
        
        public async Task<IEnumerable<Booking>> GetAllBookingsAsync()
        {
            return await _context.Bookings.ToListAsync();
        }
        
        public async Task<IEnumerable<Booking>> GetBookingsByEventIdAsync(string eventId)
        {
            return await _context.Bookings
                .Where(b => b.EventId == eventId)
                .ToListAsync();
        }
        
        public async Task<IEnumerable<Booking>> GetBookingsByUserIdAsync(string userId)
        {
            return await _context.Bookings
                .Where(b => b.UserId == userId)
                .ToListAsync();
        }
        
        public async Task<Booking?> GetBookingByIdAsync(int id)
        {
            return await _context.Bookings.FindAsync(id);
        }
        
        public async Task<Booking?> GetBookingByEventAndUserAsync(string eventId, string userId)
        {
            return await _context.Bookings
                .FirstOrDefaultAsync(b => b.EventId == eventId && b.UserId == userId);
        }
        
        public async Task<Booking> CreateBookingAsync(Booking booking)
        {
            _context.Bookings.Add(booking);
            await SaveChangesAsync();
            return booking;
        }
        
        public async Task<bool> UpdateBookingAsync(Booking booking)
        {
            _context.Entry(booking).State = EntityState.Modified;
            return await SaveChangesAsync();
        }
        
        public async Task<bool> DeleteBookingAsync(int id)
        {
            var booking = await GetBookingByIdAsync(id);
            if (booking == null)
            {
                return false;
            }
            
            _context.Bookings.Remove(booking);
            return await SaveChangesAsync();
        }
        
        public async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
} 