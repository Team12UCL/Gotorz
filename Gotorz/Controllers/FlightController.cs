using Microsoft.AspNetCore.Mvc;
using Gotorz.Services;
using Shared.Models;
using System;
using System.Threading.Tasks;

namespace Gotorz.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FlightController : ControllerBase
    {
        private readonly FlightService _flightService;

        public FlightController(FlightService flightService)
        {
            _flightService = flightService;
        }

        [HttpGet("search")]
        public async Task<ActionResult<FlightOfferRootModel>> SearchFlights(
            string originCode,
            string destinationCode,
            DateTime departureDate,
            DateTime returnDate,
            int adults = 1,
            string travelClass = "ECONOMY")
        {
            try
            {
                var result = await _flightService.SearchFlights(
                    originCode,
                    destinationCode,
                    departureDate,
                    returnDate,
                    adults,
                    travelClass);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("search/advanced")]
        public async Task<ActionResult<FlightOfferRootModel>> SearchFlightsAdvanced([FromBody] FlightSearchRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Request body cannot be null");
                }

                if (string.IsNullOrEmpty(request.OriginCode))
                {
                    return BadRequest("Origin airport code is required");
                }

                if (string.IsNullOrEmpty(request.DestinationCode))
                {
                    return BadRequest("Destination airport code is required");
                }

                var result = await _flightService.SearchFlightsAdvanced(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("search/pricing")]
        public async Task<ActionResult<FlightOfferRootModel>> SearchFlightsWithPricing([FromBody] FlightSearchRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Request body cannot be null");
                }

                if (string.IsNullOrEmpty(request.OriginCode))
                {
                    return BadRequest("Origin airport code is required");
                }

                if (string.IsNullOrEmpty(request.DestinationCode))
                {
                    return BadRequest("Destination airport code is required");
                }

                var result = await _flightService.SearchFlightsWithPricing(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}