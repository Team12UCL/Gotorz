using Shared.Models;
using Shared.Models.AmadeusCityResponse;

namespace Gotorz.Client.Services
{
    public class PricingService
    {
        public decimal CalculateTotalPrice(FlightOffer? outboundFlight, FlightOffer? returnFlight, Hotel? hotel, HotelOffer? hotelOffers)
        {
            decimal total = 0;
            total += ConvertToEUR(outboundFlight?.TotalPrice.ToString(), outboundFlight?.Currency, hotelOffers);
            total += ConvertToEUR(returnFlight?.TotalPrice.ToString(), returnFlight?.Currency, hotelOffers);
            total += ConvertToEUR(hotel?.Offers?.FirstOrDefault()?.TotalPrice.ToString(),
                                hotel?.Offers?.FirstOrDefault()?.Currency, hotelOffers);
            return total;
        }

        // Method overload for packages.razor that uses the stored conversion rates in TravelPackage.cs (since it doesn't have the hotel offers)
        public decimal ConvertToEUR(string? price, string? currency, HotelOffer? hotelOffers)
        {
            var raw = TryParsePrice(price);

            if (string.IsNullOrWhiteSpace(currency) || currency == "EUR") return raw;

            return raw;
        }


        public decimal ConvertToEUR(string? price, string? currency, Dictionary<string, decimal>? conversionRates)
        {
            var raw = TryParsePrice(price);

            // If currency is already EUR or missing, just return the raw amount
            if (string.IsNullOrWhiteSpace(currency) || currency == "EUR") return raw;

            // Use conversion rate if available
            if (conversionRates != null && conversionRates.TryGetValue(currency, out var rate))
            {
                return Math.Round(raw * rate, 2);
            }

            // If conversion not available, return raw amount
            return raw;
        }

        public decimal TryParsePrice(string? price)
        {
            return decimal.TryParse(price,
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out var result)
                ? result
                : 0;
        }
    }
}