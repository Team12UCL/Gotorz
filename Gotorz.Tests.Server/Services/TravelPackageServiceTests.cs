//using Xunit;
//using Gotorz.Client.Services;
//using Shared.Models;
//using System;
//using System.Collections.Generic;

//namespace Gotorz.Tests.Server.Services
//{
//    public class TravelPackageServiceTests
//    {
//        [Fact]
//        public void AddPackage_ShouldStorePackageCorrectly()
//        {
//            var service = new TravelPackageService();
//            var package = new TravelPackage
//            {
//                OutboundFlight = new FlightOffer
//                {
//                    Id = "Flight1",
//                    Price = new Price { Currency = "EUR", Total = "300" }
//                },
//                ReturnFlight = new FlightOffer
//                {
//                    Id = "Flight2",
//                    Price = new Price { Currency = "EUR", Total = "280" }
//                },
//                Hotel = new HotelData
//                {
//                    Hotel = new Hotel { Name = "Test Hotel" },
//                    Offers = new List<HotelOffer>
//                    {
//                        new HotelOffer
//                        {
//                            Price = new HotelPrice
//                            {
//                                Currency = "EUR",
//                                Total = "400"
//                            }
//                        }
//                    }
//                },
//                DepartureDate = new DateTime(2025, 5, 1),
//                ReturnDate = new DateTime(2025, 5, 10),
//                Adults = 2,
//                OriginCity = "Copenhagen",
//                DestinationCity = "Paris"
//            };

//            service.Packages.Add(package);

//            Assert.Single(service.Packages);
//            Assert.Equal("Copenhagen", service.Packages[0].OriginCity);
//            Assert.Equal("Paris", service.Packages[0].DestinationCity);
//            Assert.Equal("Flight1", service.Packages[0].OutboundFlight.Id);
//            Assert.Equal("Test Hotel", service.Packages[0].Hotel.Hotel.Name);
//        }

//        [Fact]
//        public void Packages_ShouldBeEmpty_OnInitialization()
//        {
//            var service = new TravelPackageService();

//            Assert.Empty(service.Packages);
//        }
//    }
//}