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
        private readonly ILogger<AmadeusAuthService> _logger;

        public AmadeusAuthService(HttpClient httpClient, IConfiguration configuration, ILogger<AmadeusAuthService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;

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
                    _logger.LogDebug("Using cached Amadeus API token");
                    return _cachedToken;
                }

                _logger.LogInformation("Getting new access token from Amadeus API");

                // Check if credentials are properly configured
                if (string.IsNullOrEmpty(_clientId) || string.IsNullOrEmpty(_clientSecret) ||
                    _clientId == "YOUR_AMADEUS_API_KEY" || _clientSecret == "YOUR_AMADEUS_API_SECRET")
                {
                    _logger.LogError("Amadeus API credentials not configured. Please update appsettings.json");
                    return null;
                }

                var requestContent = new StringContent(
                    $"grant_type=client_credentials&" +
                    $"client_id={_clientId}&" +
                    $"client_secret={_clientSecret}",
                    Encoding.UTF8,
                    "application/x-www-form-urlencoded");

                var response = await _httpClient.PostAsync(_tokenUrl, requestContent);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"Error retrieving Amadeus token: {response.StatusCode}, {responseContent}");
                    return null;
                }

                using var jsonDoc = JsonDocument.Parse(responseContent);

                // Get the token and expiration time
                _cachedToken = jsonDoc.RootElement.GetProperty("access_token").GetString();

                // Calculate expiration time (default to 30 minutes if not provided)
                if (jsonDoc.RootElement.TryGetProperty("expires_in", out var expiresIn))
                {
                    int secondsValid = expiresIn.GetInt32();
                    _tokenExpiration = DateTime.UtcNow.AddSeconds(secondsValid);
                    _logger.LogInformation($"Token retrieved, valid for {secondsValid} seconds");
                }
                else
                {
                    _tokenExpiration = DateTime.UtcNow.AddMinutes(30);
                    _logger.LogInformation("Token retrieved, using default 30 minute expiration");
                }

                return _cachedToken;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception occurred retrieving Amadeus access token: {ex.Message}");
                return null;
            }
        }
    }
}
