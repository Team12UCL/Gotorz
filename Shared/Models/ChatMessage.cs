using System;
using System.ComponentModel.DataAnnotations;

namespace Shared.Models
{
    public class ChatMessage
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid SupportTicketId { get; set; }
        public string SenderId { get; set; }

        [Required]
        public string Content { get; set; }

        public bool IsFromSupport { get; set; }
        public bool IsRead { get; set; } = false;

        // Timestamps
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
        public DateTime? ReadAt { get; set; }
    }
}