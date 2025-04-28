using Xunit;
using Moq;
using Moq.Protected;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text;
using System.Threading.Tasks;
using Server.Services;
using Shared.Models;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace Gotorz.Tests.Server.Services
{
    public class FlightServiceTests
    {
        private FlightService CreateFlightService(out Mock<HttpMessageHandler> mockHandler, out Mock<AmadeusAuthService> authServiceMock)
        {
            mockHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            authServiceMock = new Mock<AmadeusAuthService>(new HttpClient(), Mock.Of<IConfiguration>());

            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            var client = new HttpClient(mockHandler.Object);

            httpClientFactoryMock.Setup(_ => _.CreateClient("AmadeusClient")).Returns(client);

            var configurationMock = new Mock<IConfiguration>();
            configurationMock.Setup(c => c["AmadeusAPI:FlightOffersUrl"]).Returns("https://fakeflightoffers.com");

            return new FlightService(httpClientFactoryMock.Object, authServiceMock.Object, configurationMock.Object);
        }

        [Fact]
        public async Task GetFlightOffersAsync_ReturnsFlightOffers()
        {
            // Arrange
            var flightService = CreateFlightService(out var handlerMock, out var authServiceMock);

            authServiceMock.Setup(a => a.GetAccessTokenAsync()).ReturnsAsync("fake-token");

            var fakeFlightOffersJson = JsonSerializer.Serialize(new FlightOfferRootModel
            {
                Data = new List<FlightOffer>
                {
                    new FlightOffer
                    {
                        Id = "1",
                        Source = "GDS",
                        Price = new Price
                        {
                            Currency = "EUR",
                            Total = "199.99"
                        },
                        Itineraries = new List<Itinerary>
                        {
                            new Itinerary
                            {
                                Duration = "PT2H",
                                Segments = new List<Segment>
                                {
                                    new Segment
                                    {
                                        CarrierCode = "LH",
                                        Number = "123",
                                        Departure = new Departure
                                        {
                                            IataCode = "FRA",
                                            At = DateTime.UtcNow
                                        },
                                        Arrival = new Arrival
                                        {
                                            IataCode = "LHR",
                                            At = DateTime.UtcNow.AddHours(2)
                                        },
                                        Duration = "PT2H"
                                    }
                                }
                            }
                        }
                    }
                },
                Meta = new Meta()
            });

            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(fakeFlightOffersJson, Encoding.UTF8, "application/json")
                });

            // Act
            var result = await flightService.GetFlightOffersAsync("FRA", "LHR", "2025-05-01", 1);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result.Data);
            Assert.Equal("1", result.Data[0].Id);
            Assert.Equal("199.99", result.Data[0].Price.Total);
        }
    }
}