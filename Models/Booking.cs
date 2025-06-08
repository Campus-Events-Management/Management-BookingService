using System;
using System.ComponentModel.DataAnnotations;

namespace EventManagement.BookingService.Models
{
    public class Booking
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string EventId { get; set; } = string.Empty;
        
        [Required]
        public string UserId { get; set; } = string.Empty;
        
        [Required]
        public DateTime BookingDate { get; set; }
        
        public DateTime CreatedAt { get; set; }
        
        public string Status { get; set; } = "Confirmed";
        
        public string? CancellationReason { get; set; }
    }
} 