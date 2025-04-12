using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Gotorz.Services
{
    public class AmadeusAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private string _accessToken;
        private DateTime _tokenExpiration = DateTime.MinValue;

        public AmadeusAuthService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<string> GetAccessToken()
        {
            if (_accessToken != null && DateTime.UtcNow < _tokenExpiration)
            {
                return _accessToken;
            }

            var clientId = _configuration["Amadeus:ClientId"];
            var clientSecret = _configuration["Amadeus:ClientSecret"];

            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "client_credentials"),
                new KeyValuePair<string, string>("client_id", clientId),
                new KeyValuePair<string, string>("client_secret", clientSecret)
            });

            var response = await _httpClient.PostAsync("https://test.api.amadeus.com/v1/security/oauth2/token", content);
            response.EnsureSuccessStatusCode();

            var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>();
            _accessToken = tokenResponse.AccessToken;
            _tokenExpiration = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn - 60); // Buffer of 60 seconds

            return _accessToken;
        }

        private class TokenResponse
        {
            public string AccessToken { get; set; }
            public int ExpiresIn { get; set; }
        }
    }
}