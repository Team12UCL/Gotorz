using System;

namespace Shared.Models
{
    public class FlightSearchRequest
    {
        // Required parameters
        public string OriginCode { get; set; }
        public string DestinationCode { get; set; }
        public DateTime DepartureDate { get; set; }

        // Optional parameters
        public DateTime? ReturnDate { get; set; }
        public int Adults { get; set; } = 1;
        public int Children { get; set; } = 0;
        public int Infants { get; set; } = 0;

        // Travel preferences
        public string TravelClass { get; set; } = "ECONOMY"; // ECONOMY, PREMIUM_ECONOMY, BUSINESS, FIRST
        public string CurrencyCode { get; set; } = "USD";
        public int MaxResults { get; set; } = 10;

        // Airline preferences
        public string IncludedAirlineCodes { get; set; } // Comma-separated list of IATA airline codes
        public string ExcludedAirlineCodes { get; set; } // Comma-separated list of IATA airline codes

        // Additional filters
        public bool? NonStop { get; set; } // true for direct flights only
        public decimal? MaxPrice { get; set; } // Maximum price in the specified currency
    }
}