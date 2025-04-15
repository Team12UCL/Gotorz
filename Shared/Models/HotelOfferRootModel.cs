using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Shared.Models
{
    public class CityCodeResponse
    {
        public string CityCode { get; set; }
    }
    public class HotelOfferRootModel
    {
        public List<HotelData> Data { get; set; }
        public Meta Meta { get; set; }
    }

    public class HotelData
    {
        public string Type { get; set; }
        public Hotel Hotel { get; set; }
        public List<HotelOffer> Offers { get; set; }
        public Dictionary<string, string> Links { get; set; }

    }

    public class Hotel
    {
        public string Name { get; set; }
        public string CityCode { get; set; }
        public string HotelId { get; set; }
        public GeoCode GeoCode { get; set; }
        public Address Address { get; set; }
        public Contact Contact { get; set; }
        public List<string> Amenities { get; set; }
        public Media Media { get; set; }
        public string BrandCode { get; set; }
        public string BrandName { get; set; }
    }

    public class GeoCode
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }

    public class Address
    {
        public string CountryCode { get; set; }
        public string StateCode { get; set; }
        public string CityName { get; set; }
        public List<string> Lines { get; set; }
        public string PostalCode { get; set; }
    }

    public class Contact
    {
        public string Phone { get; set; }
        public string Fax { get; set; }
    }

    public class Media
    {
        public string Uri { get; set; }
        public string Category { get; set; }
    }

    public class HotelOffer
    {
        public string Id { get; set; }
        public Room Room { get; set; }
        [JsonPropertyName("price")]
        public HotelPrice Price { get; set; }
        public string CheckInDate { get; set; }
        public string CheckOutDate { get; set; }
        public string RateCode { get; set; }
        public string BoardType { get; set; }
    }

    public class Room
    {
        public string Type { get; set; }
        public TypeEstimated TypeEstimated { get; set; }
        public RoomDescription Description { get; set; }
    }

    public class RoomDescription
    {
        public string Text { get; set; }
        public string Lang { get; set; }
    }
    public class TypeEstimated
    {
        public string Category { get; set; }
        public int Beds { get; set; }
        public string BedType { get; set; }
    }


    public class HotelPrice
    {
        public string Currency { get; set; }
        public string Total { get; set; }
    }
    public class HotelIdRoot
    {
        public List<HotelIdData> Data { get; set; }
    }

    public class HotelIdData
    {
        public string Type { get; set; }
        public string HotelId { get; set; }
        public string Name { get; set; }
    }
}
