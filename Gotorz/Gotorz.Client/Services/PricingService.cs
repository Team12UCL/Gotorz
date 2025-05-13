using Shared.Models;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;
namespace Gotorz.Client.Services
{
    public class PricingService
    {
        ILogger _logger;
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
            _logger.LogCritical("--- CalculateTotalPrice method called ---");

            // Log inputs for debugging
            LogInputs(outboundFlight, returnFlight, hotel, selectedHotelOffer);

            // Add outbound flight price
            if (outboundFlight != null && outboundFlight.TotalPrice > 0)
            {
                decimal outboundPrice = ConvertToEUR(outboundFlight.TotalPrice, outboundFlight.Currency, selectedHotelOffer);
                total += outboundPrice;
                _logger.LogCritical($"Outbound flight price: {outboundPrice} (Original: {outboundFlight.TotalPrice} {outboundFlight.Currency})");
            }
            _logger.LogCritical("Total after outbound flight is " + total);

            // Add return flight price
            if (returnFlight != null && returnFlight.TotalPrice > 0)
            {
                decimal returnPrice = ConvertToEUR(returnFlight.TotalPrice, returnFlight.Currency, selectedHotelOffer);
                total += returnPrice;
                _logger.LogCritical($"Return flight price: {returnPrice} (Original: {returnFlight.TotalPrice} {returnFlight.Currency})");
            }
            _logger.LogCritical("Total after return flight is " + total);

            // Add hotel price
            var hotelOffer = selectedHotelOffer ?? hotel?.Offers?.FirstOrDefault();
            if (hotelOffer != null && hotelOffer.TotalPrice > 0)
            {
                decimal hotelPrice = ConvertToEUR(hotelOffer.TotalPrice, hotelOffer.Currency, hotelOffer);
                total += hotelPrice;
                _logger.LogCritical($"Hotel price: {hotelPrice} (Original: {hotelOffer.TotalPrice} {hotelOffer.Currency})");
            }
            _logger.LogCritical("Total after hotel is " + total);

            var finalTotal = Math.Round(total, 2, MidpointRounding.AwayFromZero);
            _logger.LogCritical($"Final total (rounded): {finalTotal}");

            return finalTotal;
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
            _logger.LogCritical("--- CalculateTotalPriceWithPassengers method called ---");

            // First calculate the base price without passenger multipliers
            decimal baseTotal = CalculateTotalPrice(outboundFlight, returnFlight, hotel, selectedHotelOffer);
            _logger.LogCritical($"Base total price (without passenger multipliers): {baseTotal}");

            // Log inputs for debugging
            LogInputs(outboundFlight, returnFlight, hotel, selectedHotelOffer, adults, children);

            // If just 1 adult and no children, return the base price
            if ((adults ?? 1) == 1 && (children ?? 0) == 0)
            {
                _logger.LogCritical($"Single adult - returning base price: {baseTotal}");
                return baseTotal;
            }

            // Calculate passenger multiplier
            decimal multiplier = (adults ?? 1) + ((children ?? 0) * 0.5m);
            _logger.LogCritical("Passenger multiplier is " + multiplier);

            // For multiple passengers, recalculate just the flight portions with the multiplier
            decimal total = 0;

            // Extract the converted flight prices from the base calculation
            decimal outboundFlightBasePrice = 0;
            decimal returnFlightBasePrice = 0;
            decimal hotelBasePrice = 0;

            // Calculate flight prices with passenger multiplier
            if (outboundFlight != null && outboundFlight.TotalPrice > 0)
            {
                outboundFlightBasePrice = ConvertToEUR(outboundFlight.TotalPrice, outboundFlight.Currency, selectedHotelOffer);
                decimal outboundPrice = outboundFlightBasePrice * multiplier;
                total += outboundPrice;
                _logger.LogCritical($"Outbound flight price with passengers: {outboundPrice} (Base: {outboundFlightBasePrice}, Multiplier: {multiplier})");
            }

            if (returnFlight != null && returnFlight.TotalPrice > 0)
            {
                returnFlightBasePrice = ConvertToEUR(returnFlight.TotalPrice, returnFlight.Currency, selectedHotelOffer);
                decimal returnPrice = returnFlightBasePrice * multiplier;
                total += returnPrice;
                _logger.LogCritical($"Return flight price with passengers: {returnPrice} (Base: {returnFlightBasePrice}, Multiplier: {multiplier})");
            }

            // Add hotel price (not multiplied by passengers as it's typically per stay)
            var hotelOffer = selectedHotelOffer ?? hotel?.Offers?.FirstOrDefault();
            if (hotelOffer != null && hotelOffer.TotalPrice > 0)
            {
                hotelBasePrice = ConvertToEUR(hotelOffer.TotalPrice, hotelOffer.Currency, hotelOffer);
                total += hotelBasePrice;
                _logger.LogCritical($"Hotel price: {hotelBasePrice} (not multiplied by passengers)");
            }

            var finalTotal = Math.Round(total, 2, MidpointRounding.AwayFromZero);
            _logger.LogCritical($"Total price with passengers (rounded): {finalTotal}");

            return finalTotal;
        }

        /// <summary>
        /// Converts price to EUR based on currency and conversion rate
        /// </summary>
        public decimal ConvertToEUR(decimal price, string? currency, HotelOffer? hotelOffer = null)
        {
            if (price <= 0)
                return 0;

            if (string.IsNullOrWhiteSpace(currency) || currency == "EUR")
                return price;

            if (hotelOffer?.ConversionRate > 0)
            {
                decimal convertedPrice = Math.Round(price * hotelOffer.ConversionRate.Value, 2);
                _logger.LogCritical($"Currency conversion: {price} {currency} → {convertedPrice} EUR (rate: {hotelOffer.ConversionRate.Value})");
                return convertedPrice;
            }

            // Default conversion if no specific rate is available
            // Log this case as it could be a source of problems
            _logger.LogWarning($"No conversion rate available for {currency} - using raw price");
            return price;
        }

        /// <summary>
        /// Helper method to log input parameters for debugging
        /// </summary>
        private void LogInputs(
            FlightOffer? outboundFlight,
            FlightOffer? returnFlight,
            Hotel? hotel,
            HotelOffer? selectedHotelOffer,
            int? adults = null,
            int? children = null)
        {
            _logger.LogCritical("--- Input Parameters ---");

            if (outboundFlight != null)
            {
                _logger.LogCritical($"Outbound Flight: ID={outboundFlight.Id}, Price={outboundFlight.TotalPrice} {outboundFlight.Currency}");
            }
            else
            {
                _logger.LogCritical("Outbound Flight: null");
            }

            if (returnFlight != null)
            {
                _logger.LogCritical($"Return Flight: ID={returnFlight.Id}, Price={returnFlight.TotalPrice} {returnFlight.Currency}");
            }
            else
            {
                _logger.LogCritical("Return Flight: null");
            }

            if (hotel != null)
            {
                _logger.LogCritical($"Hotel: ID={hotel.Id}, Name={hotel.Name}, OfferCount={hotel.Offers?.Count ?? 0}");
            }
            else
            {
                _logger.LogCritical("Hotel: null");
            }

            if (selectedHotelOffer != null)
            {
                _logger.LogCritical($"Selected Hotel Offer: ID={selectedHotelOffer.Id}, Price={selectedHotelOffer.TotalPrice} {selectedHotelOffer.Currency}, ConversionRate={selectedHotelOffer.ConversionRate}");
            }
            else
            {
                _logger.LogCritical("Selected Hotel Offer: null");
            }

            if (adults.HasValue || children.HasValue)
            {
                _logger.LogCritical($"Passengers: Adults={adults ?? 1}, Children={children ?? 0}");
            }
        }
    }
}