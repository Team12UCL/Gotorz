using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Shared.Models;
using Gotorz.Data;
using Microsoft.EntityFrameworkCore;

namespace Gotorz.Services
{
    public class TravelPackageService
    {
        private readonly FlightService _flightService;
        private readonly HotelService _hotelService;
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<TravelPackageService> _logger;

        // Collection to store bookings (for mock implementation)
        public List<Booking> Bookings { get; private set; } = new List<Booking>();

        public TravelPackageService(
            FlightService flightService,
            HotelService hotelService,
            ApplicationDbContext dbContext,
            ILogger<TravelPackageService> logger)
        {
            _flightService = flightService;
            _hotelService = hotelService;
            _dbContext = dbContext;
            _logger = logger;

            // Add some mock bookings for demonstration purposes
            if (Bookings.Count == 0)
            {
                InitializeMockBookings();
            }
        }

        private void InitializeMockBookings()
        {
            // In a real implementation, these would be stored in a database
            // Create some mock travel packages for bookings
            var package1 = new TravelPackage
            {
                Id = Guid.NewGuid(),
                Name = "Paris Getaway",
                Description = "Round-trip flight to Paris with 5 nights at Hotel de Ville",
                TotalPrice = 1250.00m,
                DepartureDate = DateTime.Now.AddDays(30),
                ReturnDate = DateTime.Now.AddDays(35),
                DurationInDays = 5,
                OriginAirport = "LAX",
                DestinationAirport = "CDG",
                Airline = "AF",
                FlightNumber = "AF65",
                HotelName = "Hotel de Ville",
                HotelAddress = "15 Rue de Rivoli, Paris",
                HotelRating = 4,
                RoomType = "Deluxe Double",
                Status = "Available",
                ImageUrl = "/images/paris.jpg",
                CreatedDate = DateTime.Now.AddDays(-15),
                OutboundFlight = MockFlightOffer("LAX", "CDG", DateTime.Now.AddDays(30)),
                ReturnFlight = MockFlightOffer("CDG", "LAX", DateTime.Now.AddDays(35)),
                Hotel = MockHotelOffer("Hotel de Ville", 4, "Paris")
            };

            var package2 = new TravelPackage
            {
                Id = Guid.NewGuid(),
                Name = "London Adventure",
                Description = "Round-trip flight to London with 7 nights at The Savoy",
                TotalPrice = 1850.00m,
                DepartureDate = DateTime.Now.AddDays(45),
                ReturnDate = DateTime.Now.AddDays(52),
                DurationInDays = 7,
                OriginAirport = "JFK",
                DestinationAirport = "LHR",
                Airline = "BA",
                FlightNumber = "BA112",
                HotelName = "The Savoy",
                HotelAddress = "Strand, London",
                HotelRating = 5,
                RoomType = "Superior King",
                Status = "Available",
                ImageUrl = "/images/london.jpg",
                CreatedDate = DateTime.Now.AddDays(-10),
                OutboundFlight = MockFlightOffer("JFK", "LHR", DateTime.Now.AddDays(45)),
                ReturnFlight = MockFlightOffer("LHR", "JFK", DateTime.Now.AddDays(52)),
                Hotel = MockHotelOffer("The Savoy", 5, "London")
            };

            var package3 = new TravelPackage
            {
                Id = Guid.NewGuid(),
                Name = "Rome Vacation",
                Description = "Round-trip flight to Rome with 4 nights at Hotel Artemide",
                TotalPrice = 1100.00m,
                DepartureDate = DateTime.Now.AddDays(-15),
                ReturnDate = DateTime.Now.AddDays(-11),
                DurationInDays = 4,
                OriginAirport = "SFO",
                DestinationAirport = "FCO",
                Airline = "AZ",
                FlightNumber = "AZ608",
                HotelName = "Hotel Artemide",
                HotelAddress = "Via Nazionale, Rome",
                HotelRating = 4,
                RoomType = "Classic Double",
                Status = "Available",
                ImageUrl = "/images/rome.jpg",
                CreatedDate = DateTime.Now.AddDays(-45),
                OutboundFlight = MockFlightOffer("SFO", "FCO", DateTime.Now.AddDays(-15)),
                ReturnFlight = MockFlightOffer("FCO", "SFO", DateTime.Now.AddDays(-11)),
                Hotel = MockHotelOffer("Hotel Artemide", 4, "Rome")
            };

            // Create some mock bookings
            Bookings.Add(new Booking
            {
                Id = 1,
                UserId = "user1",
                Package = package1,
                BookingDate = DateTime.Now.AddDays(-10),
                TravelStartDate = package1.DepartureDate,
                TravelEndDate = package1.ReturnDate,
                Status = BookingStatus.Confirmed,
                PaymentStatus = PaymentStatus.Paid,
                ReferenceNumber = "GDT-230412-1234",
                TotalAmount = package1.TotalPrice
            });

            Bookings.Add(new Booking
            {
                Id = 2,
                UserId = "user1",
                Package = package2,
                BookingDate = DateTime.Now.AddDays(-5),
                TravelStartDate = package2.DepartureDate,
                TravelEndDate = package2.ReturnDate,
                Status = BookingStatus.Pending,
                PaymentStatus = PaymentStatus.Pending,
                ReferenceNumber = "GDT-230412-5678",
                TotalAmount = package2.TotalPrice
            });

            Bookings.Add(new Booking
            {
                Id = 3,
                UserId = "user2",
                Package = package3,
                BookingDate = DateTime.Now.AddDays(-30),
                TravelStartDate = package3.DepartureDate,
                TravelEndDate = package3.ReturnDate,
                Status = BookingStatus.Completed,
                PaymentStatus = PaymentStatus.Paid,
                ReferenceNumber = "GDT-230412-9012",
                TotalAmount = package3.TotalPrice
            });
        }

        // Mock helper methods for creating test data
        private FlightOfferRootModel MockFlightOffer(string originCode, string destinationCode, DateTime departureDate)
        {
            var segment = new FlightSegment
            {
                Departure = new FlightEndpoint { IataCode = originCode, At = departureDate },
                Arrival = new FlightEndpoint { IataCode = destinationCode, At = departureDate.AddHours(8) },
                Number = "123"
            };

            var itinerary = new FlightItinerary
            {
                Segments = new List<FlightSegment> { segment }
            };

            var flightOffer = new FlightOffer
            {
                Id = "1",
                Itineraries = new List<FlightItinerary> { itinerary },
                Price = new FlightPrice { Total = 500m },
                ValidatingAirlineCodes = new List<string> { "AA" }
            };

            return new FlightOfferRootModel
            {
                Data = new List<FlightOffer> { flightOffer }
            };
        }

        private HotelOfferRootModel MockHotelOffer(string hotelName, int rating, string city)
        {
            var hotel = new HotelInfo
            {
                Name = hotelName,
                Rating = rating,
                Address = new HotelAddress { Lines = new List<string> { $"123 Main St, {city}" } },
                Media = new List<HotelMedia> { new HotelMedia { Uri = $"/images/{city.ToLower()}.jpg" } }
            };

            var offer = new HotelOfferItem
            {
                CheckInDate = DateTime.Now.AddDays(30),
                CheckOutDate = DateTime.Now.AddDays(35),
                RoomType = "Standard",
                Price = new HotelPrice { Total = "300" }
            };

            var hotelOffer = new HotelOffer
            {
                Hotel = hotel,
                Offers = new List<HotelOfferItem> { offer }
            };

            return new HotelOfferRootModel
            {
                Data = new List<HotelOffer> { hotelOffer }
            };
        }

        public async Task<TravelPackage> GetTravelPackageById(Guid id)
        {
            return await _dbContext.TravelPackages
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<List<TravelPackage>> GetAllTravelPackages()
        {
            return await _dbContext.TravelPackages.ToListAsync();
        }

        public async Task<TravelPackage> SaveTravelPackage(TravelPackage package)
        {
            if (package.Id == Guid.Empty)
            {
                package.Id = Guid.NewGuid();
                await _dbContext.TravelPackages.AddAsync(package);
            }
            else
            {
                _dbContext.TravelPackages.Update(package);
            }

            await _dbContext.SaveChangesAsync();
            return package;
        }

        public async Task<List<TravelPackage>> SearchTravelPackages(
            string originCode,
            string destinationCode,
            DateTime departureDate,
            DateTime returnDate,
            int adults = 1)
        {
            try
            {
                // First check if we have packages in the database matching these criteria
                var existingPackages = await _dbContext.TravelPackages
                    .Where(p => p.OriginAirport == originCode &&
                                p.DestinationAirport == destinationCode &&
                                p.DepartureDate.Date == departureDate.Date &&
                                p.ReturnDate.Date == returnDate.Date &&
                                p.Status == "Available")
                    .ToListAsync();

                if (existingPackages.Any())
                {
                    return existingPackages;
                }

                // If no matching packages in database, search external APIs
                // Get flights
                var flightOffers = await _flightService.SearchFlights(
                    originCode,
                    destinationCode,
                    departureDate,
                    returnDate,
                    adults);

                if (flightOffers?.Data == null || !flightOffers.Data.Any())
                {
                    return new List<TravelPackage>();
                }

                // Get hotels in the destination city
                var hotelOffers = await _hotelService.SearchHotels(
                    destinationCode.Substring(0, 3), // City code is usually the first 3 characters of airport code
                    departureDate,
                    returnDate,
                    adults);

                if (hotelOffers?.Data == null || !hotelOffers.Data.Any())
                {
                    return new List<TravelPackage>();
                }

                // Create travel packages by combining flights and hotels
                var travelPackages = new List<TravelPackage>();

                // Take top 5 flights and top 5 hotels to create 25 combinations
                var topFlights = flightOffers.Data.Take(5).ToList();
                var topHotels = hotelOffers.Data.Take(5).ToList();

                foreach (var flight in topFlights)
                {
                    foreach (var hotel in topHotels)
                    {
                        var package = CreateTravelPackage(flight, hotel, departureDate, returnDate);
                        package.OutboundFlight = new FlightOfferRootModel { Data = new List<FlightOffer> { flight } };
                        package.Hotel = new HotelOfferRootModel { Data = new List<HotelOffer> { hotel } };
                        travelPackages.Add(package);

                        // Save to database for future queries
                        await SaveTravelPackage(package);
                    }
                }

                return travelPackages.OrderBy(p => p.TotalPrice).Take(10).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching travel packages");
                return new List<TravelPackage>();
            }
        }

        private TravelPackage CreateTravelPackage(FlightOffer flight, HotelOffer hotel, DateTime departureDate, DateTime returnDate)
        {
            var hotelOffer = hotel.Offers.FirstOrDefault();
            var flightDeparture = flight.Itineraries.FirstOrDefault()?.Segments.FirstOrDefault()?.Departure;
            var flightArrival = flight.Itineraries.FirstOrDefault()?.Segments.LastOrDefault()?.Arrival;

            var flightPrice = flight.Price.Total;
            var hotelPrice = decimal.Parse(hotelOffer?.Price?.Total ?? "0");

            var durationInDays = (returnDate - departureDate).Days;

            var packageName = $"Flight to {flightArrival?.IataCode} & Stay at {hotel.Hotel.Name}";

            return new TravelPackage
            {
                Id = Guid.NewGuid(),
                Name = packageName,
                Description = $"Round-trip flight from {flightDeparture?.IataCode} to {flightArrival?.IataCode} with {flight.ValidatingAirlineCodes.FirstOrDefault()} and {durationInDays} nights at {hotel.Hotel.Name}",
                TotalPrice = flightPrice + hotelPrice,
                DepartureDate = departureDate,
                ReturnDate = returnDate,
                DurationInDays = durationInDays,
                OriginAirport = flightDeparture?.IataCode,
                DestinationAirport = flightArrival?.IataCode,
                Airline = flight.ValidatingAirlineCodes.FirstOrDefault(),
                FlightNumber = flight.Itineraries.FirstOrDefault()?.Segments.FirstOrDefault()?.Number,
                HotelName = hotel.Hotel.Name,
                HotelAddress = string.Join(", ", hotel.Hotel.Address?.Lines ?? Array.Empty<string>()),
                HotelRating = hotel.Hotel.Rating,
                RoomType = hotelOffer?.RoomType,
                Status = "Available",
                ImageUrl = hotel.Hotel.Media?.FirstOrDefault()?.Uri ?? "/images/default-hotel.jpg",
                CreatedDate = DateTime.UtcNow
            };
        }

        public async Task<bool> DeleteTravelPackage(Guid id)
        {
            var package = await _dbContext.TravelPackages.FindAsync(id);
            if (package == null)
            {
                return false;
            }

            _dbContext.TravelPackages.Remove(package);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        // Booking-related methods

        public List<Booking> GetBookingsForUser(string userId)
        {
            // In a real implementation, this would query a database
            return Bookings.Where(b => b.UserId == userId).ToList();
        }

        public Booking GetBookingByReferenceNumber(string referenceNumber)
        {
            // In a real implementation, this would query a database
            return Bookings.FirstOrDefault(b => b.ReferenceNumber == referenceNumber);
        }

        public Booking CreateBooking(string userId, TravelPackage package, DateTime travelStartDate, DateTime travelEndDate)
        {
            var booking = new Booking
            {
                Id = Bookings.Count + 1,
                UserId = userId,
                Package = package,
                BookingDate = DateTime.Now,
                TravelStartDate = travelStartDate,
                TravelEndDate = travelEndDate,
                Status = BookingStatus.Pending,
                PaymentStatus = PaymentStatus.Pending,
                ReferenceNumber = $"GDT-{DateTime.Now:yyMMdd}-{new Random().Next(1000, 9999)}",
                TotalAmount = package.TotalPrice
            };

            Bookings.Add(booking);

            return booking;
        }

        public void UpdateBookingStatus(string referenceNumber, BookingStatus status)
        {
            var booking = GetBookingByReferenceNumber(referenceNumber);
            if (booking != null)
            {
                booking.Status = status;
            }
        }

        public void UpdatePaymentStatus(string referenceNumber, PaymentStatus status)
        {
            var booking = GetBookingByReferenceNumber(referenceNumber);
            if (booking != null)
            {
                booking.PaymentStatus = status;
            }
        }
    }
}