using Shared.Models;
namespace Gotorz.Client.Services
{
    public class PricingService
    {
        public decimal CalculateTotalPrice(FlightOffer? outboundFlight, FlightOffer? returnFlight, Hotel? hotel, HotelOffer? selectedHotelOffer)
        {
            decimal total = 0;

            // Add outbound flight price
            total += ConvertToEUR(outboundFlight?.TotalPrice ?? 0, outboundFlight?.Currency);

            // Add return flight price
            total += ConvertToEUR(returnFlight?.TotalPrice ?? 0, returnFlight?.Currency);

            // Add hotel price using the selected offer or first available
            var hotelOffer = selectedHotelOffer ?? hotel?.Offers?.FirstOrDefault();
            if (hotelOffer != null)
            {
                total += ConvertToEUR(hotelOffer.TotalPrice, hotelOffer.Currency, hotelOffer);
            }

            return Math.Round(total, 2, MidpointRounding.AwayFromZero);
        }

        public decimal ConvertToEUR(decimal price, string? currency, HotelOffer? hotelOffer = null)
        {
            // If already in EUR or no currency specified, return as is
            if (string.IsNullOrWhiteSpace(currency) || currency == "EUR")
                return price;

            // Use hotel-specific conversion rate if available
            if (hotelOffer?.ConversionRate > 0)
                return Math.Round(price * hotelOffer.ConversionRate.Value, 2);

            // Default: return original price if no conversion available
            return price;
        }
    }
}