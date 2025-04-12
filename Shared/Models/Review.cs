using System;
using System.ComponentModel.DataAnnotations;

namespace Shared.Models
{
    public class Review
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid BookingId { get; set; }
        public string UserId { get; set; }

        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }

        [Required]
        [StringLength(500)]
        public string Comment { get; set; }

        public bool IsApproved { get; set; } = false;

        // Optional fields
        public int? FlightRating { get; set; }
        public int? HotelRating { get; set; }
        public int? ServiceRating { get; set; }
        public int? ValueForMoneyRating { get; set; }

        // User details (denormalized for display)
        public string UserName { get; set; }
        public string UserLocation { get; set; }

        // Photo URLs related to the review
        public string PhotoUrl1 { get; set; }
        public string PhotoUrl2 { get; set; }
        public string PhotoUrl3 { get; set; }

        // Timestamps
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}