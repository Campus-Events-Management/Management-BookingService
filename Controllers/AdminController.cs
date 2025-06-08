using EventManagement.BookingService.Models;
using EventManagement.BookingService.Models.DTOs;
using EventManagement.BookingService.Repositories;
using EventManagement.BookingService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EventManagement.BookingService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AdminController : ControllerBase
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IEventService _eventService;
        private readonly IUserService _userService;
        private readonly ILogger<AdminController> _logger;

        public AdminController(
            IBookingRepository bookingRepository,
            IEventService eventService,
            IUserService userService,
            ILogger<AdminController> logger)
        {
            _bookingRepository = bookingRepository;
            _eventService = eventService;
            _userService = userService;
            _logger = logger;
        }
        
        // Helper method to check if user is admin
        private bool IsUserAdmin()
        {
            return _userService.IsAdmin(User);
        }

        // GET: api/admin/stats
        [HttpGet("stats")]
        public async Task<ActionResult<ApiResponse<EventStatsResponseDto>>> GetEventStats()
        {
            try
            {
                // Check if user is admin
                if (!IsUserAdmin())
                {
                    _logger.LogWarning("Unauthorized access attempt to admin stats by user: {UserId}", 
                        _userService.GetUserId(User));
                    return Forbid();
                }
                
                // Get all bookings
                var allBookings = await _bookingRepository.GetAllBookingsAsync();
                
                // Group bookings by event ID
                var bookingsByEvent = allBookings.GroupBy(b => b.EventId)
                    .ToDictionary(g => g.Key, g => g.ToList());
                
                var eventStats = new List<EventStatDto>();
                
                // Process each event
                foreach (var eventId in bookingsByEvent.Keys)
                {
                    var eventBookings = bookingsByEvent[eventId];
                    var eventDetails = await _eventService.GetEventByIdAsync(eventId);
                    
                    var eventStat = new EventStatDto
                    {
                        EventId = eventId,
                        EventTitle = eventDetails?.Title ?? "Unknown Event",
                        TotalBookings = eventBookings.Count,
                        ConfirmedBookings = eventBookings.Count(b => b.Status == "Confirmed"),
                        CancelledBookings = eventBookings.Count(b => b.Status == "Cancelled"),
                        BookingRate = eventDetails != null 
                            ? (double)eventBookings.Count(b => b.Status == "Confirmed") / eventDetails.Capacity * 100
                            : 0,
                        Capacity = eventDetails?.Capacity ?? 0,
                        AvailableSeats = eventDetails != null 
                            ? eventDetails.Capacity - eventDetails.CurrentBookings
                            : 0,
                        EventDate = eventDetails?.EventDate
                    };
                    
                    eventStats.Add(eventStat);
                }
                
                // Sort events by date (upcoming first)
                eventStats = eventStats
                    .Where(e => e.EventDate.HasValue)
                    .OrderBy(e => e.EventDate)
                    .ToList();
                
                // Calculate overall statistics
                var totalEvents = eventStats.Count;
                var totalBookings = allBookings.Count();
                var confirmedBookings = allBookings.Count(b => b.Status == "Confirmed");
                var cancelledBookings = allBookings.Count(b => b.Status == "Cancelled");
                var averageBookingsPerEvent = totalEvents > 0 ? (double)totalBookings / totalEvents : 0;
                
                var response = new EventStatsResponseDto
                {
                    TotalEvents = totalEvents,
                    TotalBookings = totalBookings,
                    ConfirmedBookings = confirmedBookings,
                    CancelledBookings = cancelledBookings,
                    AverageBookingsPerEvent = averageBookingsPerEvent,
                    EventStats = eventStats
                };
                
                return Ok(ApiResponse<EventStatsResponseDto>.SuccessResponse(response));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting event statistics");
                return StatusCode(500, ApiResponse<EventStatsResponseDto>.FailureResponse(
                    "An error occurred while processing your request"));
            }
        }
        
        // GET: api/admin/stats/{eventId}
        [HttpGet("stats/{eventId}")]
        public async Task<ActionResult<ApiResponse<EventDetailStatDto>>> GetEventDetailStats(string eventId)
        {
            try
            {
                // Check if user is admin
                if (!IsUserAdmin())
                {
                    _logger.LogWarning("Unauthorized access attempt to event detail stats by user: {UserId}", 
                        _userService.GetUserId(User));
                    return Forbid();
                }
                
                // Get event details
                var eventDetails = await _eventService.GetEventByIdAsync(eventId);
                if (eventDetails == null)
                {
                    return NotFound(ApiResponse<EventDetailStatDto>.FailureResponse("Event not found"));
                }
                
                // Get bookings for this event
                var eventBookings = await _bookingRepository.GetBookingsByEventIdAsync(eventId);
                
                // Group bookings by date
                var bookingsByDate = eventBookings
                    .GroupBy(b => b.BookingDate.Date)
                    .OrderBy(g => g.Key)
                    .Select(g => new BookingDateStatDto
                    {
                        Date = g.Key,
                        BookingsCount = g.Count(),
                        ConfirmedCount = g.Count(b => b.Status == "Confirmed"),
                        CancelledCount = g.Count(b => b.Status == "Cancelled")
                    })
                    .ToList();
                
                // Group bookings by status
                var bookingsByStatus = eventBookings
                    .GroupBy(b => b.Status)
                    .ToDictionary(g => g.Key, g => g.Count());
                
                var response = new EventDetailStatDto
                {
                    EventId = eventId,
                    EventTitle = eventDetails.Title,
                    EventDescription = eventDetails.Description,
                    EventDate = eventDetails.EventDate,
                    Capacity = eventDetails.Capacity,
                    CurrentBookings = eventDetails.CurrentBookings,
                    AvailableSeats = eventDetails.Capacity - eventDetails.CurrentBookings,
                    BookingRate = eventDetails.Capacity > 0 
                        ? (double)eventDetails.CurrentBookings / eventDetails.Capacity * 100
                        : 0,
                    TotalBookingsEver = eventBookings.Count(),
                    ConfirmedBookings = bookingsByStatus.TryGetValue("Confirmed", out var confirmed) ? confirmed : 0,
                    CancelledBookings = bookingsByStatus.TryGetValue("Cancelled", out var cancelled) ? cancelled : 0,
                    BookingsByDate = bookingsByDate
                };
                
                return Ok(ApiResponse<EventDetailStatDto>.SuccessResponse(response));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting detailed statistics for event {EventId}", eventId);
                return StatusCode(500, ApiResponse<EventDetailStatDto>.FailureResponse(
                    "An error occurred while processing your request"));
            }
        }
        
        // GET: api/admin/users/{userId}/bookings
        [HttpGet("users/{userId}/bookings")]
        public async Task<ActionResult<ApiResponse<UserBookingsStatDto>>> GetUserBookings(string userId)
        {
            try
            {
                // Check if user is admin
                if (!IsUserAdmin())
                {
                    _logger.LogWarning("Unauthorized access attempt to user bookings by user: {UserId}", 
                        _userService.GetUserId(User));
                    return Forbid();
                }
                
                // Get all bookings for this user
                var userBookings = await _bookingRepository.GetBookingsByUserIdAsync(userId);
                
                // Map bookings to response DTOs with event details
                var bookingDtos = new List<BookingWithEventDto>();
                
                foreach (var booking in userBookings)
                {
                    var eventDetails = await _eventService.GetEventByIdAsync(booking.EventId);
                    
                    bookingDtos.Add(new BookingWithEventDto
                    {
                        BookingId = booking.Id,
                        EventId = booking.EventId,
                        EventTitle = eventDetails?.Title ?? "Unknown Event",
                        BookingDate = booking.BookingDate,
                        CreatedAt = booking.CreatedAt,
                        Status = booking.Status,
                        CancellationReason = booking.CancellationReason,
                        EventDate = eventDetails?.EventDate
                    });
                }
                
                // Sort bookings by event date (upcoming first)
                bookingDtos = bookingDtos
                    .OrderBy(b => b.EventDate)
                    .ToList();
                
                // Calculate statistics
                var totalBookings = userBookings.Count();
                var confirmedBookings = userBookings.Count(b => b.Status == "Confirmed");
                var cancelledBookings = userBookings.Count(b => b.Status == "Cancelled");
                
                var response = new UserBookingsStatDto
                {
                    UserId = userId,
                    TotalBookings = totalBookings,
                    ConfirmedBookings = confirmedBookings,
                    CancelledBookings = cancelledBookings,
                    Bookings = bookingDtos
                };
                
                return Ok(ApiResponse<UserBookingsStatDto>.SuccessResponse(response));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting bookings for user {UserId}", userId);
                return StatusCode(500, ApiResponse<UserBookingsStatDto>.FailureResponse(
                    "An error occurred while processing your request"));
            }
        }
    }
} 