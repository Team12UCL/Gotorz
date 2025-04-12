using System;
using System.ComponentModel.DataAnnotations;

namespace Shared.Models
{
    public class Booking
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public string UserId { get; set; }
        public Guid TravelPackageId { get; set; }

        [Required]
        public string BookingReference { get; set; }

        public DateTime BookingDate { get; set; } = DateTime.UtcNow;
        public string Status { get; set; } = "Pending"; // Pending, Confirmed, Cancelled, Completed

        public int NumberOfTravelers { get; set; }
        public decimal TotalAmount { get; set; }

        // Traveler details
        public string PrimaryContactName { get; set; }
        public string PrimaryContactEmail { get; set; }
        public string PrimaryContactPhone { get; set; }

        // Special requests
        public string SpecialRequests { get; set; }

        // Payment status
        public bool IsPaid { get; set; } = false;
        public DateTime? PaymentDate { get; set; }

        // Timestamps
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}