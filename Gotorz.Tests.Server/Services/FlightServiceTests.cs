using Xunit;
using Server.Services;
using Shared.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc.Testing;
using Gotorz;

namespace Gotorz.Tests.Server.Services
{
    public class FlightServiceTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public FlightServiceTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task GetFlightOffersAsync_WithValidParams_ReturnsExpectedData()
        {
            using var scope = _factory.Services.CreateScope();
            var flightService = scope.ServiceProvider.GetRequiredService<FlightService>();

            string origin = "CPH";
            string destination = "PAR";
            string departureDate = DateTime.UtcNow.AddDays(30).ToString("yyyy-MM-dd");
            int adults = 1;

            var offers = await flightService.GetFlightOffersAsync(origin, destination, departureDate, adults);

            Assert.NotNull(offers);
            Assert.NotEmpty(offers);

            foreach (var offer in offers)
            {
                Assert.False(string.IsNullOrWhiteSpace(offer.OfferId));
                Assert.False(string.IsNullOrWhiteSpace(offer.AirlineCode));
                Assert.True(offer.TotalPrice > 0);
                Assert.True(offer.BasePrice > 0);
                Assert.False(string.IsNullOrWhiteSpace(offer.Currency));
                Assert.True(offer.AvailableSeats > 0);
                Assert.NotNull(offer.Itineraries);
                Assert.All(offer.Itineraries, itinerary =>
                {
                    Assert.False(string.IsNullOrWhiteSpace(itinerary.Duration));
                    Assert.NotNull(itinerary.Segments);
                    Assert.NotEmpty(itinerary.Segments);

                    foreach (var segment in itinerary.Segments)
                    {
                        Assert.False(string.IsNullOrWhiteSpace(segment.DepartureAirport));
                        Assert.False(string.IsNullOrWhiteSpace(segment.ArrivalAirport));
                        Assert.False(string.IsNullOrWhiteSpace(segment.CarrierCode));
                        Assert.False(string.IsNullOrWhiteSpace(segment.FlightNumber));
                        Assert.True(segment.DepartureTime < segment.ArrivalTime);
                    }
                });
            }
        }
    }
}