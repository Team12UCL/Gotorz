using System.Diagnostics;
using System.Text.Json;
using System.Text;

namespace Server.Services
{
    public class AmadeusAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly string _tokenUrl;
        private readonly string _clientId;
        private readonly string _clientSecret;

        public AmadeusAuthService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;

            _tokenUrl = _configuration["AmadeusAPI:TokenUrl"]!;
            _clientId = _configuration["AmadeusAPI:ClientId"]!;
            _clientSecret = _configuration["AmadeusAPI:ClientSecret"]!;
        }

        public virtual async Task<string?> GetAccessTokenAsync()
        {
            var requestContent = new StringContent(
                $"grant_type=client_credentials&" +
                $"client_id={_clientId}&" +
                $"client_secret={_clientSecret}",
                Encoding.UTF8,
                "application/x-www-form-urlencoded");

            var response = await _httpClient.PostAsync(_tokenUrl, requestContent);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            Debug.WriteLine("Token retrieved");
            var responseContent = await response.Content.ReadAsStringAsync();
            using var jsonDoc = JsonDocument.Parse(responseContent);
            return jsonDoc.RootElement.GetProperty("access_token").GetString();
        }
    }
}
