using Shared.Models;
using System;

namespace Gotorz.Client.Services
{
    public class PricingService
    {
        private readonly ILogger _logger;

        public PricingService(ILogger<PricingService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Calculates the total price for a package with flights and hotel (without passenger multipliers)
        /// </summary>
        public decimal CalculateTotalPrice(
            FlightOffer? outboundFlight,
            FlightOffer? returnFlight,
            Hotel? hotel,
            HotelOffer? selectedHotelOffer)
        {
            decimal total = 0;

            // Add outbound flight price
            _logger.LogInformation($"Adding outbound flight price: {outboundFlight?.TotalPrice} {outboundFlight?.Currency}");
            total += AddFlightPrice(outboundFlight, selectedHotelOffer);

            // Add return flight price
            _logger.LogInformation($"Adding return flight price: {returnFlight?.TotalPrice} {returnFlight?.Currency}");
            total += AddFlightPrice(returnFlight, selectedHotelOffer);

            // Add hotel price
            _logger.LogInformation($"Adding hotel price");
            total += AddHotelPrice(hotel, selectedHotelOffer);

            _logger.LogInformation($"Total calculated price (before rounding): {total}");
            return Math.Round(total, 2, MidpointRounding.AwayFromZero);
        }

        /// <summary>
        /// Calculates the total price including passenger multipliers for flights
        /// </summary>
        public decimal CalculateTotalPriceWithPassengers(
            FlightOffer? outboundFlight,
            FlightOffer? returnFlight,
            Hotel? hotel,
            HotelOffer? selectedHotelOffer,
            int? adults = 1,
            int? children = 0)
        {
            decimal baseTotal = CalculateTotalPrice(outboundFlight, returnFlight, hotel, selectedHotelOffer);
            _logger.LogInformation($"Base total price: {baseTotal}");

            if ((adults ?? 1) == 1 && (children ?? 0) == 0)
                return baseTotal;

            decimal multiplier = (adults ?? 1) + ((children ?? 0) * 0.5m);
            _logger.LogInformation($"Passenger multiplier applied: {multiplier}");

            decimal total = 0;
            total += AddFlightPriceWithMultiplier(outboundFlight, selectedHotelOffer, multiplier);
            total += AddFlightPriceWithMultiplier(returnFlight, selectedHotelOffer, multiplier);

            _logger.LogInformation($"Total after multiplier application: {total}");
            total += AddHotelPrice(hotel, selectedHotelOffer);

            _logger.LogInformation($"Final total price (before rounding): {total}");
            return Math.Round(total, 2, MidpointRounding.AwayFromZero);
        }

        private decimal AddFlightPrice(FlightOffer? flight, HotelOffer? selectedHotelOffer)
        {
            if (flight?.TotalPrice > 0)
            {
                _logger.LogInformation($"Converting flight price: {flight.TotalPrice} {flight.Currency}");
                return ConvertToEUR(flight.TotalPrice, flight.Currency, selectedHotelOffer);
            }
            return 0;
        }

        private decimal AddFlightPriceWithMultiplier(FlightOffer? flight, HotelOffer? selectedHotelOffer, decimal multiplier)
        {
            if (flight?.TotalPrice > 0)
            {
                decimal basePrice = ConvertToEUR(flight.TotalPrice, flight.Currency, selectedHotelOffer);
                _logger.LogInformation($"Price after conversion and multiplier application: {basePrice} * {multiplier}");
                return basePrice * multiplier;
            }
            return 0;
        }

        private decimal AddHotelPrice(Hotel? hotel, HotelOffer? selectedHotelOffer)
        {
            if (selectedHotelOffer != null)
            {
                _logger.LogInformation($"Adding selected hotel price: {selectedHotelOffer.TotalPrice} {selectedHotelOffer.Currency}");
                return ConvertToEUR(selectedHotelOffer.TotalPrice, selectedHotelOffer.Currency, selectedHotelOffer);
            }

            if (hotel?.Offers?.Any() == true)
            {
                var fallbackHotelOffer = hotel.Offers.First();
                _logger.LogInformation($"Adding fallback hotel price: {fallbackHotelOffer.TotalPrice} {fallbackHotelOffer.Currency}");
                return ConvertToEUR(fallbackHotelOffer.TotalPrice, fallbackHotelOffer.Currency, fallbackHotelOffer);
            }

            return 0;
        }

        private readonly Dictionary<string, decimal> _conversionCache = new Dictionary<string, decimal>();

        public decimal ConvertToEUR(decimal price, string? currency, HotelOffer? hotelOffer = null)
        {
            if (price <= 0 || string.IsNullOrWhiteSpace(currency) || currency == "EUR")
                return price;

            string cacheKey = $"{price}-{currency}";

            // ✅ Check if it's already converted
            if (_conversionCache.ContainsKey(cacheKey))
            {
                _logger.LogInformation($"✅ Using cached conversion for {price} {currency}: {_conversionCache[cacheKey]} EUR");
                return _conversionCache[cacheKey];
            }

            decimal convertedPrice;

            // ✅ Perform conversion
            if (hotelOffer?.ConversionRate > 0)
            {
                convertedPrice = Math.Round(price * hotelOffer.ConversionRate.Value, 2);
            }
            else
            {
                convertedPrice = price;
            }

            // ✅ Store in cache
            _conversionCache[cacheKey] = convertedPrice;
            _logger.LogInformation($"💶 Converted {price} {currency} to {convertedPrice} EUR");

            return convertedPrice;
        }
    }
}