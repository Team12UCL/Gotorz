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
    }
}