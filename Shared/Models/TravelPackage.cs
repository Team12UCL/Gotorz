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
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTime DepartureDate { get; set; }
        public DateTime ReturnDate { get; set; }
        public int DurationInDays { get; set; }

        // Flight details (bundled)
        public string OriginAirport { get; set; }
        public string DestinationAirport { get; set; }
        public string Airline { get; set; }
        public string FlightNumber { get; set; }

        // Hotel details (bundled)
        public string HotelName { get; set; }
        public string HotelAddress { get; set; }
        public int HotelRating { get; set; }
        public string RoomType { get; set; }

        // Booking status
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string ImageUrl { get; set; }
    }
}

