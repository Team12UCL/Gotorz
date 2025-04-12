using System;
using System.Collections.Generic;

namespace Shared.Models
{
    public class AirportRootModel
    {
        public List<Airport> Data { get; set; }
    }

    public class Airport
    {
        public string IataCode { get; set; }
        public string Name { get; set; }
        public string CityName { get; set; }
        public string CountryName { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}