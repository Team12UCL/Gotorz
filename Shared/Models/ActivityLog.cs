using System;
using System.ComponentModel.DataAnnotations;

namespace Shared.Models
{
	public class ActivityLog
	{
		[Key]
		public Guid Id { get; set; } = Guid.NewGuid();

		[Required]
		public string UserId { get; set; }

		[Required]
		public string ActivityType { get; set; }  // Login, Booking, Payment, AdminAction, etc.

		[Required]
		public string Description { get; set; }

		public DateTime Timestamp { get; set; } = DateTime.UtcNow;

		public string IpAddress { get; set; }

		// Additional metadata as needed
		public string UserAgent { get; set; }
		public string SessionId { get; set; }
		public string ResourceId { get; set; }  // ID of affected resource (booking, payment, etc.)
		public string ResourceType { get; set; }  // Type of affected resource

		public bool ShowDetails { get; set; } = false;  // Flag to show/hide details in the UI
	}
}