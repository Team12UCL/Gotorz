using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models
{
    public class TravelPackage
    {
        public FlightOffer Flight { get; set; }
        public HotelData Hotel { get; set; }
        public DateTime DepartureDate { get; set; }
    }
}
