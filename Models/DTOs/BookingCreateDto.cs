using System;
using System.ComponentModel.DataAnnotations;

namespace EventManagement.BookingService.Models.DTOs
{
    public class BookingCreateDto
    {
        [Required]
        public string EventId { get; set; } = string.Empty;
        
        // Optional: Client can send the booking date, otherwise server sets it to Now
        public DateTime? BookingDate { get; set; }
    }
} 