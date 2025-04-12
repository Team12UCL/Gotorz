using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Gotorz.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Shared.Models;

namespace Gotorz.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TravelPackageController : ControllerBase
    {
        private readonly TravelPackageService _travelPackageService;
        private readonly ILogger<TravelPackageController> _logger;

        public TravelPackageController(
            TravelPackageService travelPackageService,
            ILogger<TravelPackageController> logger)
        {
            _travelPackageService = travelPackageService;
            _logger = logger;
        }

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<TravelPackage>>> SearchTravelPackages(
            [FromQuery] string originCode,
            [FromQuery] string destinationCode,
            [FromQuery] DateTime departureDate,
            [FromQuery] DateTime returnDate,
            [FromQuery] int adults = 1)
        {
            try
            {
                var packages = await _travelPackageService.SearchTravelPackages(
                    originCode,
                    destinationCode,
                    departureDate,
                    returnDate,
                    adults);

                return Ok(packages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching travel packages");
                return StatusCode(500, "An error occurred while searching for travel packages.");
            }
        }

        [HttpGet("{id}")]
        public ActionResult<TravelPackage> GetTravelPackage(Guid id)
        {
            // In a real implementation, this would fetch from a database
            // For now, return a placeholder message
            return NotFound($"Package with ID {id} not found. Database implementation pending.");
        }
    }
}