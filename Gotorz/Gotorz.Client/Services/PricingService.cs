using Shared.Models;
using Shared.Models.AmadeusCityResponse;

namespace Gotorz.Client.Services
{
    public class PricingService
    {
        public decimal CalculateTotalPrice(FlightOffer? outboundFlight, FlightOffer? returnFlight, Hotel? hotel, HotelOffer? hotelOffer)
        {
            decimal total = 0;

            total += ConvertToEUR(outboundFlight?.TotalPrice ?? 0, outboundFlight?.Currency, hotelOffer);
            total += ConvertToEUR(returnFlight?.TotalPrice ?? 0, returnFlight?.Currency, hotelOffer);

            var hotelPrice = hotel?.Offers?.FirstOrDefault()?.TotalPrice ?? 0;
            var hotelCurrency = hotel?.Offers?.FirstOrDefault()?.Currency ?? "EUR";
            total += ConvertToEUR(hotelPrice, hotelCurrency, hotelOffer);

            return total;
        }

        public decimal ConvertToEUR(decimal price, string? currency, HotelOffer? hotelOffer)
        {
            if (string.IsNullOrWhiteSpace(currency) || currency == "EUR")
                return price;

            var rate = hotelOffer?.ConversionRate ?? 0;
            if (rate != 0)
                return Math.Round(price * rate, 2, MidpointRounding.AwayFromZero);

            return price;
        }
    }
}
