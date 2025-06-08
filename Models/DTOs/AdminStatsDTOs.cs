using System;
using System.Collections.Generic;

namespace EventManagement.BookingService.Models.DTOs
{
    // Overall event statistics response
    public class EventStatsResponseDto
    {
        public int TotalEvents { get; set; }
        public int TotalBookings { get; set; }
        public int ConfirmedBookings { get; set; }
        public int CancelledBookings { get; set; }
        public double AverageBookingsPerEvent { get; set; }
        public List<EventStatDto> EventStats { get; set; } = new List<EventStatDto>();
    }

    // Statistics for a single event
    public class EventStatDto
    {
        public string EventId { get; set; } = string.Empty;
        public string EventTitle { get; set; } = string.Empty;
        public int TotalBookings { get; set; }
        public int ConfirmedBookings { get; set; }
        public int CancelledBookings { get; set; }
        public double BookingRate { get; set; } // Percentage of capacity booked
        public int Capacity { get; set; }
        public int AvailableSeats { get; set; }
        public DateTime? EventDate { get; set; }
    }

    // Detailed statistics for a single event
    public class EventDetailStatDto
    {
        public string EventId { get; set; } = string.Empty;
        public string EventTitle { get; set; } = string.Empty;
        public string EventDescription { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public int Capacity { get; set; }
        public int CurrentBookings { get; set; }
        public int AvailableSeats { get; set; }
        public double BookingRate { get; set; } // Percentage of capacity booked
        public int TotalBookingsEver { get; set; } // Including cancelled
        public int ConfirmedBookings { get; set; }
        public int CancelledBookings { get; set; }
        public List<BookingDateStatDto> BookingsByDate { get; set; } = new List<BookingDateStatDto>();
    }

    // Booking statistics by date
    public class BookingDateStatDto
    {
        public DateTime Date { get; set; }
        public int BookingsCount { get; set; }
        public int ConfirmedCount { get; set; }
        public int CancelledCount { get; set; }
    }

    // User booking statistics
    public class UserBookingsStatDto
    {
        public string UserId { get; set; } = string.Empty;
        public int TotalBookings { get; set; }
        public int ConfirmedBookings { get; set; }
        public int CancelledBookings { get; set; }
        public List<BookingWithEventDto> Bookings { get; set; } = new List<BookingWithEventDto>();
    }

    // Booking with event details
    public class BookingWithEventDto
    {
        public int BookingId { get; set; }
        public string EventId { get; set; } = string.Empty;
        public string EventTitle { get; set; } = string.Empty;
        public DateTime BookingDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? CancellationReason { get; set; }
        public DateTime? EventDate { get; set; }
    }
} 