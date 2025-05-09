using Shared.Models;
namespace Gotorz.Client.Services
{
    public class PricingService
    {
        public decimal CalculateTotalPrice(
            FlightOffer? outboundFlight,
            FlightOffer? returnFlight,
            Hotel? hotel,
            HotelOffer? selectedHotelOffer,
            int? adults = null,
            int? children = null)
        {
            decimal total = 0;
            decimal multiplier = 1;

            if (adults.HasValue || children.HasValue)
            {
                multiplier = (adults ?? 0) + ((children ?? 0) * 0.5m);
            }

            // Add outbound flight price
            total += ConvertToEUR(outboundFlight?.TotalPrice ?? 0, outboundFlight?.Currency, selectedHotelOffer) * multiplier;

            // Add return flight price
            total += ConvertToEUR(returnFlight?.TotalPrice ?? 0, returnFlight?.Currency, selectedHotelOffer) * multiplier;

            // Add hotel price (usually per stay, not per person)
            var hotelOffer = selectedHotelOffer ?? hotel?.Offers?.FirstOrDefault();
            if (hotelOffer != null)
            {
                total += ConvertToEUR(hotelOffer.TotalPrice, hotelOffer.Currency, hotelOffer);
            }

            return Math.Round(total, 2, MidpointRounding.AwayFromZero);
        }

        public decimal ConvertToEUR(decimal price, string? currency, HotelOffer? hotelOffer = null)
        {
            if (string.IsNullOrWhiteSpace(currency) || currency == "EUR")
                return price;

            if (hotelOffer?.ConversionRate > 0)
                return Math.Round(price * hotelOffer.ConversionRate.Value, 2);

            return price;
        }
    }

}