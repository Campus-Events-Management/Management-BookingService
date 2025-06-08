using EventManagement.BookingService.Models.DTOs;
using System.Threading.Tasks;

namespace EventManagement.BookingService.Services
{
    public interface IEventService
    {
        Task<EventDto?> GetEventByIdAsync(string eventId);
        Task<bool> UpdateEventBookingCountAsync(string eventId, bool isIncrement);
    }
} 