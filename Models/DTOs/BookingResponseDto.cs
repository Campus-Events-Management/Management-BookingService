using System;

namespace EventManagement.BookingService.Models.DTOs
{
    public class BookingResponseDto
    {
        public int Id { get; set; }
        public string EventId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public DateTime BookingDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? CancellationReason { get; set; }
        
        // Additional properties that might be populated from Event Service
        public string? EventTitle { get; set; }
        public string? EventDescription { get; set; }
        public DateTime? EventDate { get; set; }
    }
} 