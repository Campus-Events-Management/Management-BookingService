using EventManagement.BookingService.Models;
using EventManagement.BookingService.Models.DTOs;
using EventManagement.BookingService.Repositories;
using EventManagement.BookingService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventManagement.BookingService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BookingsController : ControllerBase
    {
        private readonly IBookingRepository _repository;
        private readonly IEventService _eventService;
        private readonly IUserService _userService;
        private readonly ILogger<BookingsController> _logger;
        
        public BookingsController(
            IBookingRepository repository,
            IEventService eventService,
            IUserService userService,
            ILogger<BookingsController> logger)
        {
            _repository = repository;
            _eventService = eventService;
            _userService = userService;
            _logger = logger;
        }
        
        // GET: api/bookings
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<ApiResponse<IEnumerable<BookingResponseDto>>>> GetBookings([FromQuery] string? eventId)
        {
            try
            {
                IEnumerable<Booking> bookings;
                
                if (!string.IsNullOrEmpty(eventId))
                {
                    // Check if user is admin for eventId query
                    if (!_userService.IsAdmin(User))
                    {
                        return Unauthorized(ApiResponse<IEnumerable<BookingResponseDto>>.FailureResponse(
                            "Only administrators can view all bookings for an event"));
                    }
                    
                    bookings = await _repository.GetBookingsByEventIdAsync(eventId);
                }
                else
                {
                    // Get bookings for current user
                    string userId = _userService.GetUserId(User);
                    bookings = await _repository.GetBookingsByUserIdAsync(userId);
                }
                
                var bookingDtos = new List<BookingResponseDto>();
                
                foreach (var booking in bookings)
                {
                    var bookingDto = MapToBookingResponseDto(booking);
                    
                    // Try to get event details from Event Service
                    var eventDetails = await _eventService.GetEventByIdAsync(booking.EventId);
                    if (eventDetails != null)
                    {
                        bookingDto.EventTitle = eventDetails.Title;
                        bookingDto.EventDescription = eventDetails.Description;
                        bookingDto.EventDate = eventDetails.EventDate;
                    }
                    
                    bookingDtos.Add(bookingDto);
                }
                
                return Ok(ApiResponse<IEnumerable<BookingResponseDto>>.SuccessResponse(bookingDtos));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting bookings");
                return StatusCode(500, ApiResponse<IEnumerable<BookingResponseDto>>.FailureResponse(
                    "An error occurred while processing your request"));
            }
        }
        
        // GET: api/bookings/check/{eventId}
        [HttpGet("check/{eventId}")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<bool>>> CheckBookingExists(string eventId)
        {
            try
            {
                string userId = _userService.GetUserId(User);
                var booking = await _repository.GetBookingByEventAndUserAsync(eventId, userId);
                
                bool hasBooking = booking != null;
                
                return Ok(ApiResponse<bool>.SuccessResponse(hasBooking, 
                    hasBooking ? "You have already booked this event" : "You have not booked this event yet"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking booking existence for event ID {EventId}", eventId);
                return StatusCode(500, ApiResponse<bool>.FailureResponse(
                    "An error occurred while processing your request"));
            }
        }
        
        // GET: api/bookings/5
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<BookingResponseDto>>> GetBooking(int id)
        {
            try
            {
                var booking = await _repository.GetBookingByIdAsync(id);
                
                if (booking == null)
                {
                    return NotFound(ApiResponse<BookingResponseDto>.FailureResponse("Booking not found"));
                }
                
                // Check if user owns this booking or is admin
                string userId = _userService.GetUserId(User);
                if (booking.UserId != userId && !_userService.IsAdmin(User))
                {
                    return Unauthorized(ApiResponse<BookingResponseDto>.FailureResponse(
                        "You are not authorized to view this booking"));
                }
                
                var bookingDto = MapToBookingResponseDto(booking);
                
                // Try to get event details from Event Service
                var eventDetails = await _eventService.GetEventByIdAsync(booking.EventId);
                if (eventDetails != null)
                {
                    bookingDto.EventTitle = eventDetails.Title;
                    bookingDto.EventDescription = eventDetails.Description;
                    bookingDto.EventDate = eventDetails.EventDate;
                }
                
                return Ok(ApiResponse<BookingResponseDto>.SuccessResponse(bookingDto));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting booking with ID {BookingId}", id);
                return StatusCode(500, ApiResponse<BookingResponseDto>.FailureResponse(
                    "An error occurred while processing your request"));
            }
        }
        
        // POST: api/bookings
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<ApiResponse<BookingResponseDto>>> CreateBooking(BookingCreateDto bookingDto)
        {
            try
            {
                _logger.LogInformation("Creating booking for event ID: {EventId}", bookingDto.EventId);
                
                // Check if event exists and has capacity
                var eventDetails = await _eventService.GetEventByIdAsync(bookingDto.EventId);
                if (eventDetails == null)
                {
                    _logger.LogWarning("Event not found with ID: {EventId}", bookingDto.EventId);
                    return BadRequest(ApiResponse<BookingResponseDto>.FailureResponse("Event not found"));
                }
                
                if (eventDetails.IsCapacityReached)
                {
                    _logger.LogWarning("Event at full capacity: {EventId}", bookingDto.EventId);
                    return BadRequest(ApiResponse<BookingResponseDto>.FailureResponse("Event is at full capacity"));
                }
                
                // Get user ID from token
                string userId = _userService.GetUserId(User);
                _logger.LogInformation("User ID from token: {UserId}", userId);
                
                // Check if user already has a booking for this event
                var existingBooking = await _repository.GetBookingByEventAndUserAsync(bookingDto.EventId, userId);
                if (existingBooking != null)
                {
                    _logger.LogWarning("User already has a booking for this event. UserId: {UserId}, EventId: {EventId}", 
                        userId, bookingDto.EventId);
                    return BadRequest(ApiResponse<BookingResponseDto>.FailureResponse("You already have a booking for this event"));
                }
                
                // Create booking
                var booking = new Booking
                {
                    EventId = bookingDto.EventId,
                    UserId = userId,
                    BookingDate = bookingDto.BookingDate ?? DateTime.Now,
                    CreatedAt = DateTime.Now,
                    Status = "Confirmed"
                };
                
                // First update event booking count
                bool eventUpdated = await _eventService.UpdateEventBookingCountAsync(booking.EventId, true);
                if (!eventUpdated)
                {
                    _logger.LogError("Failed to update event booking count. EventId: {EventId}", booking.EventId);
                    return StatusCode(500, ApiResponse<BookingResponseDto>.FailureResponse(
                        "Failed to update event capacity. Please try again later."));
                }
                
                // Then create the booking
                var createdBooking = await _repository.CreateBookingAsync(booking);
                _logger.LogInformation("Created booking with ID: {BookingId}", createdBooking.Id);
                
                var responseDto = MapToBookingResponseDto(createdBooking);
                responseDto.EventTitle = eventDetails.Title;
                responseDto.EventDescription = eventDetails.Description;
                responseDto.EventDate = eventDetails.EventDate;
                
                return CreatedAtAction(nameof(GetBooking), new { id = createdBooking.Id }, 
                    ApiResponse<BookingResponseDto>.SuccessResponse(responseDto, "Booking created successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating booking for event ID {EventId}", bookingDto.EventId);
                return StatusCode(500, ApiResponse<BookingResponseDto>.FailureResponse(
                    "An error occurred while processing your request"));
            }
        }
        
        // DELETE: api/bookings/5
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteBooking(int id)
        {
            try
            {
                var booking = await _repository.GetBookingByIdAsync(id);
                
                if (booking == null)
                {
                    return NotFound(ApiResponse<bool>.FailureResponse("Booking not found"));
                }
                
                // Check if user owns this booking or is admin
                string userId = _userService.GetUserId(User);
                if (booking.UserId != userId && !_userService.IsAdmin(User))
                {
                    return Unauthorized(ApiResponse<bool>.FailureResponse(
                        "You are not authorized to delete this booking"));
                }
                
                // First update event booking count
                bool eventUpdated = await _eventService.UpdateEventBookingCountAsync(booking.EventId, false);
                if (!eventUpdated)
                {
                    _logger.LogWarning("Failed to update event booking count during cancellation. EventId: {EventId}", booking.EventId);
                    // Continue with deletion anyway
                }
                
                // Delete booking
                bool result = await _repository.DeleteBookingAsync(id);
                
                if (result)
                {
                    return Ok(ApiResponse<bool>.SuccessResponse(true, "Booking cancelled successfully"));
                }
                else
                {
                    return StatusCode(500, ApiResponse<bool>.FailureResponse("Failed to cancel booking"));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting booking with ID {BookingId}", id);
                return StatusCode(500, ApiResponse<bool>.FailureResponse(
                    "An error occurred while processing your request"));
            }
        }
        
        // Helper method to map Booking to BookingResponseDto
        private static BookingResponseDto MapToBookingResponseDto(Booking booking)
        {
            return new BookingResponseDto
            {
                Id = booking.Id,
                EventId = booking.EventId,
                UserId = booking.UserId,
                BookingDate = booking.BookingDate,
                CreatedAt = booking.CreatedAt,
                Status = booking.Status,
                CancellationReason = booking.CancellationReason
            };
        }
    }
} 