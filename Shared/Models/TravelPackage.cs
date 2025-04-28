using Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models
{
    public class TravelPackage
    {
        public FlightOffer OutboundFlight {get; set; }
        public FlightOffer? ReturnFlight {get; set; }
        public HotelData Hotel { get; set; }
        public HotelOfferRootModel HotelOfferRoot { get; set; } // Added, because HotelData data structure does not contain pricing conversions
        public DateTime DepartureDate { get; set; }
        public DateTime ReturnDate { get; set; }
        public int Adults { get; set; }
        //public decimal Price { get; set; }
        public string? OriginCity { get; set; }
        public string? DestinationCity { get; set; }

        public Dictionary<string, decimal> CurrencyConversionRates { get; set; } = new();
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}

//public class TravelPackage
//{
//    public FlightOffer Flight { get; set; }
//    public HotelData Hotel { get; set; }
//    public DateTime DepartureDate { get; set; }
//}

