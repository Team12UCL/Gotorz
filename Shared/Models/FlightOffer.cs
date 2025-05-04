using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models
{
	public class FlightOffer
	{
		[Key]
		public int Id { get; set; }  // Required for EF Core to track the entity

		public string OfferId { get; set; }
		public string AirlineCode { get; set; }
		public decimal TotalPrice { get; set; }
		public decimal BasePrice { get; set; }
		public string Currency { get; set; }
		public int AvailableSeats { get; set; }
		public ICollection<Itinerary> Itineraries { get; set; }
	}

	public class Itinerary
	{
		[Key]
		public int Id { get; set; }
		public int ItineraryId { get; set; }
		public string Duration { get; set; }
		public int FlightOfferId { get; set; }
		public ICollection<FlightSegment> Segments { get; set; }
	}

	public class FlightSegment
	{
		[Key]
		public int Id { get; set; }
		public int FlightSegmentId { get; set; }
		public string DepartureAirport { get; set; }
		public DateTime DepartureTime { get; set; }
		public string ArrivalAirport { get; set; }
		public DateTime ArrivalTime { get; set; }
		public string CarrierCode { get; set; }
		public string FlightNumber { get; set; }
		public string AircraftCode { get; set; }
		public int Stops { get; set; }
		public string CabinClass { get; set; }
		public int CheckedBags { get; set; }
	}

}
