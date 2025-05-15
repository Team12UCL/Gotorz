using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models
{
	public class Hotel
	{
		[Key]
		public Guid Id { get; set; } = Guid.NewGuid();

		public string? ExternalHotelId { get; set; }  // Amadeus API ID

		public string? Name { get; set; }
		public string? CityCode { get; set; }
		public double Latitude { get; set; }
		public double Longitude { get; set; }

		public ICollection<HotelOffer>? Offers { get; set; }
	}

	public class HotelOffer
	{
		[Key]
		public Guid Id { get; set; } = Guid.NewGuid();

		public string? OfferId { get; set; }          // Amadeus API ID

		public Guid HotelDbId { get; set; }           // Foreign key to Hotel.Id
		public DateTime CheckInDate { get; set; }
		public DateTime CheckOutDate { get; set; }
		public string? RoomType { get; set; }
		public string? RoomCategory { get; set; }
		public string? BedType { get; set; }
		public int NumberOfBeds { get; set; }
		public string? Description { get; set; }
		public decimal BasePrice { get; set; }
		public decimal TotalPrice { get; set; }
		public string Currency { get; set; }
        public decimal? ConversionRate { get; set; }
        public string CancellationPolicy { get; set; }
	}

	public class CityCodeResponse    {	
	public Guid Id { get; set; } = Guid.NewGuid();
	public string CityCode { get; set; }
  }
}
