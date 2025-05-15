//using Xunit;
//using Shared.Models;
//using Microsoft.EntityFrameworkCore;
//using Gotorz.Data;
//using Gotorz.Services;
//using System;
//using System.Collections.Generic;
//using System.Threading.Tasks;

//namespace Gotorz.Tests.Server.Services
//{
//    public class TravelPackageServiceTests
//    {
//        private ApplicationDbContext CreateInMemoryDbContext()
//        {
//            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
//                .UseInMemoryDatabase(Guid.NewGuid().ToString())
//                .Options;

//            return new ApplicationDbContext(options);
//        }

//        [Fact]
//        public async Task CreateAsync_ShouldAddPackageToDatabase()
//        {
//            var dbContext = CreateInMemoryDbContext();
//            var service = new TravelPackageService(dbContext);

//            var package = new TravelPackage
//            {
//                OutboundFlight = new FlightOffer
//                {
//                    OfferId = "F123",
//                    AirlineCode = "LH",
//                    TotalPrice = 250,
//                    BasePrice = 220,
//                    Currency = "EUR",
//                    AvailableSeats = 5,
//                    Itineraries = new List<Itinerary>()
//                },
//                ReturnFlight = new FlightOffer
//                {
//                    OfferId = "F124",
//                    AirlineCode = "LH",
//                    TotalPrice = 240,
//                    BasePrice = 210,
//                    Currency = "EUR",
//                    AvailableSeats = 5,
//                    Itineraries = new List<Itinerary>()
//                },
//                Hotel = new Hotel
//                {
//                    Name = "Test Hotel",
//                    CityCode = "PAR",
//                    Latitude = 48.8566,
//                    Longitude = 2.3522,
//                    Offers = new List<HotelOffer>()
//                },
//                DepartureDate = new DateTime(2025, 5, 1),
//                ReturnDate = new DateTime(2025, 5, 10),
//                Adults = 2,
//                OriginCity = "Copenhagen",
//                DestinationCity = "Paris",
//                Name = "Romantic Escape",
//                Status = TravelPackageStatus.Available
//            };

//            var result = await service.CreateAsync(package);
//            var all = await service.GetAllAsync();

//            Assert.NotNull(result);
//            Assert.Single(all);
//            Assert.Equal("Romantic Escape", all[0].Name);
//            Assert.Equal("Test Hotel", all[0].Hotel.Name);
//        }

//        [Fact]
//        public async Task GetAllAsync_InitiallyEmpty()
//        {
//            var dbContext = CreateInMemoryDbContext();
//            var service = new TravelPackageService(dbContext);

//            var all = await service.GetAllAsync();

//            Assert.Empty(all);
//        }
//    }
//}