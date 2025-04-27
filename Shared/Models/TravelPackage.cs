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

		public FlightOffer OutboundFlight {get; set; }
        public FlightOffer? ReturnFlight {get; set; }
        public HotelData Hotel { get; set; }
        public DateTime DepartureDate { get; set; }
        public DateTime ReturnDate { get; set; }
        public int Adults { get; set; }
        //public decimal Price { get; set; }
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

//public class TravelPackage
//{
//    public FlightOffer Flight { get; set; }
//    public HotelData Hotel { get; set; }
//    public DateTime DepartureDate { get; set; }
//}

