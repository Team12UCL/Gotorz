using Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Shared.Models
{
	public class TravelPackage
	{
		[Key]
		public Guid TravelPackageId { get; set; } = Guid.NewGuid();

		// Outbound flight
		public Guid OutboundFlightId { get; set; }
		public FlightOffer OutboundFlight { get; set; }

		// Return flight (optional)
		public Guid ReturnFlightId { get; set; }
		public FlightOffer ReturnFlight { get; set; }

		// Hotel
		public Guid HotelId { get; set; }
		public Hotel Hotel { get; set; }

		public DateTime DepartureDate { get; set; }
		public DateTime ReturnDate { get; set; }

		public int Adults { get; set; }

		public string? OriginCity { get; set; }
		public string? DestinationCity { get; set; }

		public string? Name { get; set; }
		public string? Description { get; set; }

		public TravelPackageStatus Status { get; set; }
	}


	public enum TravelPackageStatus
	{
		Available,
        Limited,
        Sold_Out
	}
}
