using System;
using System.ComponentModel.DataAnnotations;

namespace Shared.Models
{
    public class Notification
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public string UserId { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Message { get; set; }

        public string Type { get; set; } // Info, Success, Warning, Error
        public bool IsRead { get; set; } = false;

        // Optional related entities
        public Guid? BookingId { get; set; }
        public Guid? PaymentId { get; set; }
        public Guid? SupportTicketId { get; set; }

        // Action link
        public string ActionLink { get; set; }
        public string ActionText { get; set; }

        // Timestamps
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ReadAt { get; set; }
    }
}