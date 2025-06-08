using System;

namespace EventManagement.BookingService.Models.DTOs
{
    public class EventDto
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public string Location { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public int CurrentBookings { get; set; }
        public bool IsCapacityReached => CurrentBookings >= Capacity;
    }
} 