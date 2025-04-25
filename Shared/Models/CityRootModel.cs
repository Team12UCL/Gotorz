using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.AmadeusCityResponse
{
    public class AmadeusCityResponse
    {
        public Meta Meta { get; set; }
        public List<CityData> Data { get; set; }
    }

    public class Meta
    {
        public int Count { get; set; }
        public Links Links { get; set; }
    }

    public class Links
    {
        public string Self { get; set; }
    }

    public class CityData
    {
        public string Type { get; set; }
        public string SubType { get; set; }
        public string Name { get; set; }
        public string IataCode { get; set; }
        public Address Address { get; set; }
        public GeoCode GeoCode { get; set; }
    }

    public class Address
    {
        public string CountryCode { get; set; }
        public string StateCode { get; set; }
    }

    public class GeoCode
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }

}
