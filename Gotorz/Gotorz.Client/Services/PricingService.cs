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
        //overload metode for calculating adults and children
        public decimal CalculateTotalPrice(FlightOffer? outboundFlight, FlightOffer? returnFlight, Hotel? hotel, HotelOffer? hotelOffers, int adults, int children = 0)
        {
            decimal total = 0;
            decimal multiplier = adults + (children * 0.5m);

            
            total += ConvertToEUR(outboundFlight?.TotalPrice.ToString(), outboundFlight?.Currency, hotelOffers) * multiplier;
            total += ConvertToEUR(returnFlight?.TotalPrice.ToString(), returnFlight?.Currency, hotelOffers) * multiplier;

            
            total += ConvertToEUR(hotel?.Offers?.FirstOrDefault()?.TotalPrice.ToString(),
                                  hotel?.Offers?.FirstOrDefault()?.Currency, hotelOffers);

            return Math.Round(total, 2);
        }


        // Method overload for packages.razor that uses the stored conversion rates in TravelPackage.cs (since it doesn't have the hotel offers)
        //public decimal CalculateTotalPrice(FlightOffer? outboundFlight,
        //                            FlightOffer? returnFlight,
        //                            Hotel? hotel, Dictionary<string, decimal>? conversionRates,
        //                            int travelers = 1)
        //{
        //    decimal total = 0;
        //    // Multiply flight prices by number of travelers
        //    total += ConvertToEUR(outboundFlight?.Price?.Total, outboundFlight?.Price?.Currency, conversionRates) * travelers;
        //    total += ConvertToEUR(returnFlight?.Price?.Total, returnFlight?.Price?.Currency, conversionRates) * travelers;
        //    // Hotel price is per stay, not per person
        //    total += ConvertToEUR(hotel?.Offers?.FirstOrDefault()?.Price?.Total,
        //                        hotel?.Offers?.FirstOrDefault()?.Price?.Currency, conversionRates);
        //    return total;
        //}

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