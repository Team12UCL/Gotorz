using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.DTO
{
	public class TravelPackageDTO
	{


			[Key]
			public Guid TravelPackageId { get; set; } = Guid.NewGuid();

			// Outbound flight
			public int OutboundFlightId { get; set; }
		

			// Return flight (optional)
			public int? ReturnFlightId { get; set; }
			

			// Hotel
			public int HotelId { get; set; }
		

			public DateTime DepartureDate { get; set; }
			public DateTime ReturnDate { get; set; }

			public int Adults { get; set; }

			public string? OriginCity { get; set; }
			public string? DestinationCity { get; set; }

			public string? Name { get; set; }
			public string? Description { get; set; }

			public TravelPackageStatus Status { get; set; }
	
	}
}
