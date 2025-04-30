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
            var service = new AirportService();
            var query = "Copenhagen";

            var result = await service.SearchAirportsAsync(query);

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
            var service = new AirportService();
            var emptyQuery = "";

            var result = await service.SearchAirportsAsync(emptyQuery);

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task SearchAirportsAsync_WithNoMatches_ReturnsEmptyList()
        {
            var service = new AirportService();
            var query = "Nonexistent Airport";

            var result = await service.SearchAirportsAsync(query);

            Assert.NotNull(result);
            Assert.Empty(result);
        }
    }
}