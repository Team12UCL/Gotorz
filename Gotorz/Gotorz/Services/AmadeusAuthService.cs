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
        private string? _cachedToken;
        private DateTime _tokenExpiration = DateTime.MinValue;

        public AmadeusAuthService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;

            _tokenUrl = _configuration["AmadeusAPI:TokenUrl"]!;
            _clientId = _configuration["AmadeusAPI:ClientId"]!;
            _clientSecret = _configuration["AmadeusAPI:ClientSecret"]!;
        }

        public async Task<string?> GetAccessTokenAsync()
        {
            try
            {
                // Check if we have a cached token that's still valid (with 30 second buffer)
                if (!string.IsNullOrEmpty(_cachedToken) && _tokenExpiration > DateTime.UtcNow.AddSeconds(30))
                {
                    Debug.WriteLine("Using cached token");
                    return _cachedToken;
                }

                Debug.WriteLine("Getting new access token from Amadeus API");

                // Check if credentials are properly configured
                if (string.IsNullOrEmpty(_clientId) || string.IsNullOrEmpty(_clientSecret) ||
                    _clientId == "YOUR_AMADEUS_API_KEY" || _clientSecret == "YOUR_AMADEUS_API_SECRET")
                {
                    Debug.WriteLine("ERROR: Amadeus API credentials not configured. Please update appsettings.json");
                    return null;
                }

                var requestContent = new StringContent(
                    $"grant_type=client_credentials&" +
                    $"client_id={_clientId}&" +
                    $"client_secret={_clientSecret}",
                    Encoding.UTF8,
                    "application/x-www-form-urlencoded");

                var response = await _httpClient.PostAsync(_tokenUrl, requestContent);
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine($"Error retrieving token: {response.StatusCode}, {errorContent}");
                    return null;
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                using var jsonDoc = JsonDocument.Parse(responseContent);

                // Get the token and expiration time
                _cachedToken = jsonDoc.RootElement.GetProperty("access_token").GetString();

                // Calculate expiration time (default to 30 minutes if not provided)
                if (jsonDoc.RootElement.TryGetProperty("expires_in", out var expiresIn))
                {
                    int secondsValid = expiresIn.GetInt32();
                    _tokenExpiration = DateTime.UtcNow.AddSeconds(secondsValid);
                    Debug.WriteLine($"Token retrieved, valid for {secondsValid} seconds");
                }
                else
                {
                    _tokenExpiration = DateTime.UtcNow.AddMinutes(30);
                    Debug.WriteLine("Token retrieved, using default 30 minute expiration");
                }

                return _cachedToken;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception occurred retrieving access token: {ex.Message}");
                return null;
            }
        }
    }
}
