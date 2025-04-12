using Microsoft.AspNetCore.Mvc;
using Server.Services;
using Shared.Models;
using System.Diagnostics;

namespace Gotorz.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AirportController : ControllerBase
    {
        private readonly AirportService _airportService;
        private readonly ILogger<AirportController> _logger;

        public AirportController(AirportService airportService, ILogger<AirportController> logger)
        {
            _airportService = airportService;
            _logger = logger;
        }

        // POST: api/airport/add-airports
        [HttpPost("add-airports")]
        public async Task<IActionResult> SearchAndAddAirports([FromBody] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                _logger.LogWarning("Empty query received for add-airports");
                return BadRequest("Query cannot be empty.");
            }

            _logger.LogInformation($"Adding airports for query: {query}");
            var results = await _airportService.PersistAirportsToJsonAsync(query);

            return Ok(new { message = "Airports added.", count = results.Count });
        }

        // GET: api/airport/suggest-airports
        [HttpGet("suggest-airports")]
        public async Task<ActionResult<List<Airport>>> SuggestAirports([FromQuery] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                _logger.LogWarning("Empty query received for suggest-airports");
                return BadRequest("Query is required.");
            }

            _logger.LogInformation($"Suggesting airports for query: {query}");
            var airports = await _airportService.SearchAirportsAsync(query);

            if (airports.Count == 0)
            {
                _logger.LogInformation($"No airports found for query: {query}");
            }
            else
            {
                _logger.LogInformation($"Found {airports.Count} airports for query: {query}");
            }

            return Ok(airports);
        }
    }
}