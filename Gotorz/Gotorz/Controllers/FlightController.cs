using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Server.Services;

namespace Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FlightController : ControllerBase
    {
        private readonly FlightService _flightService;
        private readonly ILogger<FlightController> _logger;

        public FlightController(FlightService flightService, ILogger<FlightController> logger)
        {
            _flightService = flightService;
            _logger = logger;
        }

        // GET: api/flight/search?originLocationCode=CDG&destinationLocationCode=MAD&departureDate=2023-12-01&returnDate=2023-12-08&adults=2&travelClass=BUSINESS&nonStop=true
        [HttpGet("search")]
        public async Task<IActionResult> GetFlightOffers(
            [FromQuery] string originLocationCode,
            [FromQuery] string destinationLocationCode,
            [FromQuery] string departureDate,
            [FromQuery] string? returnDate = null,
            [FromQuery] int adults = 1,
            [FromQuery] string travelClass = "ECONOMY",
            [FromQuery] bool nonStop = false,
            [FromQuery] string currencyCode = "EUR",
            [FromQuery] int max = 10)
        {
            _logger.LogInformation($"Searching flights from {originLocationCode} to {destinationLocationCode} on {departureDate} for {adults} adults");

            if (string.IsNullOrWhiteSpace(originLocationCode) || string.IsNullOrWhiteSpace(destinationLocationCode))
            {
                _logger.LogWarning("Missing origin or destination in flight search request");
                return BadRequest("Origin and destination location codes are required.");
            }

            if (string.IsNullOrWhiteSpace(departureDate))
            {
                _logger.LogWarning("Missing departure date in flight search request");
                return BadRequest("Departure date is required.");
            }

            var flightOffers = await _flightService.GetFlightOffersAsync(
                originLocationCode,
                destinationLocationCode,
                departureDate,
                adults,
                returnDate,
                travelClass,
                nonStop,
                currencyCode,
                max);

            if (flightOffers == null)
            {
                _logger.LogError($"Error retrieving flight offers from {originLocationCode} to {destinationLocationCode}");
                return StatusCode(500, "An error occurred while fetching flight offers.");
            }

            _logger.LogInformation($"Found {flightOffers.Data?.Count ?? 0} flight offers from {originLocationCode} to {destinationLocationCode}");
            return Ok(flightOffers);
        }
    }
}
