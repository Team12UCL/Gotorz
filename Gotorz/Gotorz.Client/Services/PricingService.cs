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
            total += AddFlightPrice(outboundFlight, selectedHotelOffer);
            // Add return flight price
            total += AddFlightPrice(returnFlight, selectedHotelOffer);
            // Add hotel price
            total += AddHotelPrice(hotel, selectedHotelOffer);

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
            // Calculate base total price
            decimal baseTotal = CalculateTotalPrice(outboundFlight, returnFlight, hotel, selectedHotelOffer);

            // If just 1 adult and no children, return the base price
            if ((adults ?? 1) == 1 && (children ?? 0) == 0)
                return baseTotal;

            // Calculate passenger multiplier
            decimal multiplier = (adults ?? 1) + ((children ?? 0) * 0.5m);

            // Recalculate flight portions with the multiplier
            decimal total = 0;
            total += AddFlightPriceWithMultiplier(outboundFlight, selectedHotelOffer, multiplier);
            total += AddFlightPriceWithMultiplier(returnFlight, selectedHotelOffer, multiplier);

            // Add hotel price (not multiplied by passengers)
            total += AddHotelPrice(hotel, selectedHotelOffer);

            return Math.Round(total, 2, MidpointRounding.AwayFromZero);
        }

        private decimal AddFlightPrice(FlightOffer? flight, HotelOffer? selectedHotelOffer)
        {
            if (flight?.TotalPrice > 0)
            {
                return ConvertToEUR(flight.TotalPrice, flight.Currency, selectedHotelOffer);
            }
            return 0;
        }

        private decimal AddFlightPriceWithMultiplier(FlightOffer? flight, HotelOffer? selectedHotelOffer, decimal multiplier)
        {
            if (flight?.TotalPrice > 0)
            {
                decimal basePrice = ConvertToEUR(flight.TotalPrice, flight.Currency, selectedHotelOffer);
                return basePrice * multiplier;
            }
            return 0;
        }

        private decimal AddHotelPrice(Hotel? hotel, HotelOffer? selectedHotelOffer)
        {
            if (selectedHotelOffer != null)
            {
                return ConvertToEUR(selectedHotelOffer.TotalPrice, selectedHotelOffer.Currency, selectedHotelOffer);
            }

            // If no selected hotel offer, fallback to the first hotel offer
            if (hotel?.Offers?.Any() == true)
            {
                var fallbackHotelOffer = hotel.Offers.First();
                return ConvertToEUR(fallbackHotelOffer.TotalPrice, fallbackHotelOffer.Currency, fallbackHotelOffer);
            }

            return 0;
        }

        /// <summary>
        /// Converts price to EUR based on currency and conversion rate
        /// </summary>
        public decimal ConvertToEUR(decimal price, string? currency, HotelOffer? hotelOffer = null)
        {
            if (price <= 0 || string.IsNullOrWhiteSpace(currency) || currency == "EUR")
                return price;

            if (hotelOffer?.ConversionRate > 0)
            {
                return Math.Round(price * hotelOffer.ConversionRate.Value, 2);
            }

            // Default conversion if no specific rate is available
            return price;
        }
    }
}
