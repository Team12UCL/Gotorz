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

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TravelPackage>>> GetAllTravelPackages()
        {
            try
            {
                var packages = await _travelPackageService.GetAllTravelPackages();
                return Ok(packages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all travel packages");
                return StatusCode(500, "An error occurred while retrieving travel packages.");
            }
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
        public async Task<ActionResult<TravelPackage>> GetTravelPackage(Guid id)
        {
            try
            {
                var package = await _travelPackageService.GetTravelPackageById(id);

                if (package == null)
                {
                    return NotFound($"Package with ID {id} not found.");
                }

                return Ok(package);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving travel package with ID {id}");
                return StatusCode(500, "An error occurred while retrieving the travel package.");
            }
        }

        [HttpPost]
        public async Task<ActionResult<TravelPackage>> CreateTravelPackage([FromBody] TravelPackage package)
        {
            try
            {
                if (package == null)
                {
                    return BadRequest("Travel package data is required.");
                }

                var savedPackage = await _travelPackageService.SaveTravelPackage(package);
                return CreatedAtAction(nameof(GetTravelPackage), new { id = savedPackage.Id }, savedPackage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating travel package");
                return StatusCode(500, "An error occurred while creating the travel package.");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTravelPackage(Guid id, [FromBody] TravelPackage package)
        {
            try
            {
                if (package == null || id != package.Id)
                {
                    return BadRequest("Invalid travel package data or ID mismatch.");
                }

                var existingPackage = await _travelPackageService.GetTravelPackageById(id);
                if (existingPackage == null)
                {
                    return NotFound($"Package with ID {id} not found.");
                }

                await _travelPackageService.SaveTravelPackage(package);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating travel package with ID {id}");
                return StatusCode(500, "An error occurred while updating the travel package.");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTravelPackage(Guid id)
        {
            try
            {
                var result = await _travelPackageService.DeleteTravelPackage(id);
                if (!result)
                {
                    return NotFound($"Package with ID {id} not found.");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting travel package with ID {id}");
                return StatusCode(500, "An error occurred while deleting the travel package.");
            }
        }
    }
}