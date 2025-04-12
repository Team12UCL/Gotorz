using System;
using System.Collections.Generic;

namespace Shared.Models
{
    public class HotelOfferRootModel
    {
        public List<HotelOffer> Data { get; set; }
    }

    public class HotelOffer
    {
        public string Type { get; set; }
        public Hotel Hotel { get; set; }
        public List<Offer> Offers { get; set; }
        public string Id { get; set; }
    }

    public class Hotel
    {
        public string Type { get; set; }
        public string HotelId { get; set; }
        public string ChainCode { get; set; }
        public string BrandCode { get; set; }
        public string DupeId { get; set; }
        public string Name { get; set; }
        public string CityCode { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public HotelDistance HotelDistance { get; set; }
        public Address Address { get; set; }
        public Contact Contact { get; set; }
        public string Description { get; set; }
        public List<Media> Media { get; set; }
        public List<Amenity> Amenities { get; set; }
        public int Rating { get; set; }
    }

    public class HotelDistance
    {
        public string Description { get; set; }
        public double Distance { get; set; }
        public string DistanceUnit { get; set; }
    }

    public class Address
    {
        public List<string> Lines { get; set; }
        public string PostalCode { get; set; }
        public string CityName { get; set; }
        public string CountryCode { get; set; }
    }

    public class Contact
    {
        public string Phone { get; set; }
        public string Fax { get; set; }
        public string Email { get; set; }
    }

    public class Media
    {
        public string Uri { get; set; }
        public string Category { get; set; }
    }

    public class Amenity
    {
        public string Code { get; set; }
        public string Description { get; set; }
    }

    public class Offer
    {
        public string Id { get; set; }
        public string RoomType { get; set; }
        public string Description { get; set; }
        public List<BoardType> Board { get; set; }
        public string CheckInDate { get; set; }
        public string CheckOutDate { get; set; }
        public int Adults { get; set; }
        public HotelPrice Price { get; set; }
        public Policies Policies { get; set; }
    }

    public class BoardType
    {
        public string Code { get; set; }
        public string Description { get; set; }
    }

    public class Policies
    {
        public List<CancellationPolicy> Cancellation { get; set; }
    }

    public class CancellationPolicy
    {
        public string Type { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public DateTime Deadline { get; set; }
    }

    public class HotelPrice
    {
        public string Currency { get; set; }
        public string Total { get; set; }
    }
}
