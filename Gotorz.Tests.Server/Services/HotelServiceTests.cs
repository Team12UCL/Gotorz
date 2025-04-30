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
    public class HotelServiceTests
    {
        private HotelService CreateHotelService(out Mock<HttpMessageHandler> mockHandler, out Mock<AmadeusAuthService> authServiceMock)
        {
            mockHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            authServiceMock = new Mock<AmadeusAuthService>(new HttpClient(), Mock.Of<IConfiguration>());

            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            var client = new HttpClient(mockHandler.Object);

            httpClientFactoryMock.Setup(_ => _.CreateClient("AmadeusClient")).Returns(client);

            var configurationMock = new Mock<IConfiguration>();
            configurationMock.Setup(c => c["AmadeusAPI:HotelOffersUrl"]).Returns("https://fakehoteloffers.com");
            configurationMock.Setup(c => c["AmadeusAPI:HotelsByCityUrl"]).Returns("https://fakehotelsbycity.com");

            return new HotelService(httpClientFactoryMock.Object, authServiceMock.Object, configurationMock.Object);
        }

        [Fact]
        public async Task GetHotelOffersAsync_ReturnsOffers()
        {
            var hotelService = CreateHotelService(out var handlerMock, out var authServiceMock);

            authServiceMock.Setup(a => a.GetAccessTokenAsync()).ReturnsAsync("fake-token");

            var fakeHotelIdsJson = @"{
                ""data"": [
                    { ""hotelId"": ""HOTEL123"" },
                    { ""hotelId"": ""HOTEL456"" }
                ]
            }";

            var fakeHotelOffersJson = JsonSerializer.Serialize(new HotelOfferRootModel
            {
                Data = new List<HotelData>
                {
                    new HotelData
                    {
                        Hotel = new Hotel { Name = "Test Hotel", CityCode = "PAR" },
                        Offers = new List<HotelOffer>
                        {
                            new HotelOffer
                            {
                                Price = new HotelPrice { Currency = "EUR", Total = "123.45" },
                                CheckInDate = "2025-05-01",
                                CheckOutDate = "2025-05-05",
                                BoardType = "Breakfast"
                            }
                        }
                    }
                },
                Meta = new Meta(),
                Dictionaries = new Dictionaries()
            });

            var responses = new Queue<HttpResponseMessage>(new[]
            {
                new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(fakeHotelIdsJson, Encoding.UTF8, "application/json")
                },
                new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(fakeHotelOffersJson, Encoding.UTF8, "application/json")
                }
            });

            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(() => responses.Dequeue());

            var result = await hotelService.GetHotelOffersAsync("PAR", "2025-05-01", "2025-05-05", 2);

            Assert.NotNull(result);
            Assert.NotEmpty(result.Data);
            Assert.Equal("Test Hotel", result.Data[0].Hotel.Name);
        }
    }
}