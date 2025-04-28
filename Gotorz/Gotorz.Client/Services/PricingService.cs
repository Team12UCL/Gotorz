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