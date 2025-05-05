using Shared.Models;
using Shared.Models.AmadeusCityResponse;

namespace Gotorz.Client.Services
{
    public class PricingService
    {
        public decimal CalculateTotalPrice(FlightOffer? outboundFlight, FlightOffer? returnFlight, HotelData? hotel, HotelOfferRootModel? hotelOffers)
        {
            decimal total = 0;
            total += ConvertToEUR(outboundFlight?.Price?.Total, outboundFlight?.Price?.Currency, hotelOffers);
            total += ConvertToEUR(returnFlight?.Price?.Total, returnFlight?.Price?.Currency, hotelOffers);
            total += ConvertToEUR(hotel?.Offers?.FirstOrDefault()?.Price?.Total,
                                hotel?.Offers?.FirstOrDefault()?.Price?.Currency, hotelOffers);
            return total;
        }

        // Method overload for packages.razor that uses the stored conversion rates in TravelPackage.cs (since it doesn't have the hotel offers)
        public decimal CalculateTotalPrice(FlightOffer? outboundFlight,
                                    FlightOffer? returnFlight,
                                    HotelData? hotel, Dictionary<string, decimal>? conversionRates,
                                    int travelers = 1)
        {
            decimal total = 0;
            // Multiply flight prices by number of travelers
            total += ConvertToEUR(outboundFlight?.Price?.Total, outboundFlight?.Price?.Currency, conversionRates) * travelers;
            total += ConvertToEUR(returnFlight?.Price?.Total, returnFlight?.Price?.Currency, conversionRates) * travelers;
            // Hotel price is per stay, not per person
            total += ConvertToEUR(hotel?.Offers?.FirstOrDefault()?.Price?.Total,
                                hotel?.Offers?.FirstOrDefault()?.Price?.Currency, conversionRates);
            return total;
        }

        public decimal ConvertToEUR(string? price, string? currency, HotelOfferRootModel? hotelOffers)
        {
            var raw = TryParsePrice(price);

            // If currency is already EUR or missing, just return the raw amount
            if (string.IsNullOrWhiteSpace(currency) || currency == "EUR") return raw;

            // Try to get conversion rate from HotelOffers if available
            var conversion = hotelOffers?.Dictionaries?.CurrencyConversionLookupRates?.GetValueOrDefault(currency);
            if (conversion == null || conversion.Target != "EUR") return raw;

            if (decimal.TryParse(conversion.Rate, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out var rate))
            {
                return Math.Round(raw * rate, conversion.TargetDecimalPlaces);
            }

            return raw;
        }

        // Method overload for packages.razor that uses the stored conversion rates in TravelPackage.cs (since it doesn't have the hotel offers)
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

            return raw; // If conversion not available, return raw amount
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