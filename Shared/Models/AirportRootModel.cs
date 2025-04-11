using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.AirportRootModel
{
    public class AirportRootModel
    {
        //public Meta Meta { get; set; }
        public List<Location> Data { get; set; } = new();
    }

    //public class Meta
    //{
    //    public int Count { get; set; }
    //    public Links Links { get; set; }
    //}

    //public class Links
    //{
    //    public string Self { get; set; }
    //    public string Next { get; set; }
    //    public string Last { get; set; }
    //}

    public class Location
    {
        public string Name { get; set; }
        public string IataCode { get; set; }
        public Address Address { get; set; }
    }

    public class Address
    {
        public string CityName { get; set; }
        public string CountryName { get; set; }
    }
}