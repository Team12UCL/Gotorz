using Xunit;
using Server.Services;
using Shared.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Gotorz.Tests.Server.Services
{
    public class AirportServiceTests
    {
        [Fact]
        public async Task SearchAirportsAsync_ReturnsMatchingAirports()
        {
            // Arrange
            var service = new AirportService(); // You are loading from airports.json
            var query = "Copenhagen"; // Example search query

            // Act
            var result = await service.SearchAirportsAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.All(result, airport =>
                Assert.True(
                    airport.Name.Contains(query, System.StringComparison.OrdinalIgnoreCase) ||
                    airport.City.Contains(query, System.StringComparison.OrdinalIgnoreCase) ||
                    airport.Country.Contains(query, System.StringComparison.OrdinalIgnoreCase) ||
                    airport.IataCode.Contains(query, System.StringComparison.OrdinalIgnoreCase)
                )
            );
        }

        [Fact]
        public async Task SearchAirportsAsync_WithEmptyQuery_ReturnsEmptyList()
        {
            // Arrange
            var service = new AirportService();
            var emptyQuery = "";

            // Act
            var result = await service.SearchAirportsAsync(emptyQuery);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task SearchAirportsAsync_WithNoMatches_ReturnsEmptyList()
        {
            // Arrange
            var service = new AirportService();
            var query = "Nonexistent Airport";

            // Act
            var result = await service.SearchAirportsAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }
    }
}