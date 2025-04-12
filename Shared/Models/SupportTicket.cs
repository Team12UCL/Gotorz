using System;
using System.ComponentModel.DataAnnotations;

namespace Shared.Models
{
    public class SupportTicket
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public string UserId { get; set; }

        public Guid? BookingId { get; set; } // Optional, may not be related to a specific booking

        [Required]
        public string TicketNumber { get; set; }

        [Required]
        public string Subject { get; set; }

        [Required]
        public string Description { get; set; }

        public string Status { get; set; } = "Open"; // Open, InProgress, Resolved, Closed
        public string Priority { get; set; } = "Medium"; // Low, Medium, High, Urgent

        public string Category { get; set; } // Booking, Payment, Cancellation, General, Technical

        // Staff assigned to this ticket
        public string AssignedToUserId { get; set; }

        // Timestamps
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }
    }
}