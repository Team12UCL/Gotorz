using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using Server.Services;
using Shared.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Gotorz.Tests.Server.Services
{
    public class HotelServiceTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HotelService _hotelService;

        public HotelServiceTests(WebApplicationFactory<Program> factory)
        {
            using var scope = factory.Services.CreateScope();
            _hotelService = scope.ServiceProvider.GetRequiredService<HotelService>();
        }

        [Fact]
        public async Task GetHotelOffersAsync_WithValidInputs_ReturnsResults()
        {
            var cityCode = "PAR";
            var checkInDate = DateTime.UtcNow.AddDays(30).ToString("yyyy-MM-dd");
            var checkOutDate = DateTime.UtcNow.AddDays(33).ToString("yyyy-MM-dd");
            var adults = 2;

            var hotels = await _hotelService.GetHotelOffersAsync(cityCode, checkInDate, checkOutDate, adults);

            Assert.NotNull(hotels);
            Assert.NotEmpty(hotels);

            foreach (var hotel in hotels)
            {
                Assert.False(string.IsNullOrWhiteSpace(hotel.Name));
                Assert.False(string.IsNullOrWhiteSpace(hotel.CityCode));
                Assert.True(hotel.Latitude != 0);
                Assert.True(hotel.Longitude != 0);
                Assert.NotNull(hotel.Offers);
                Assert.NotEmpty(hotel.Offers);

                foreach (var offer in hotel.Offers)
                {
                    Assert.True(offer.TotalPrice > 0);
                    Assert.False(string.IsNullOrWhiteSpace(offer.Currency));
                    Assert.True(offer.CheckInDate < offer.CheckOutDate);
                    Assert.False(string.IsNullOrWhiteSpace(offer.RoomType));
                }
            }
        }

        [Fact]
        public async Task GetCitySuggestionsAsync_WithQuery_ReturnsCities()
        {
            var result = await _hotelService.GetCitySuggestionsAsync("Paris");

            Assert.NotNull(result);
            Assert.NotEmpty(result);
            Assert.Contains(result, c => c.IataCode == "PAR");
        }

        [Fact]
        public async Task GetCityCodeAsync_WithValidCityName_ReturnsCode()
        {
            var cityCode = await _hotelService.GetCityCodeAsync("Paris");

            Assert.NotNull(cityCode);
            Assert.Equal("PAR", cityCode);
        }
    }
}