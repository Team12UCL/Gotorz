using Microsoft.AspNetCore.Mvc;
using Gotorz.Services;
using Shared.Models;
using System.Diagnostics;

namespace Gotorz.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TravelPackageController : ControllerBase
    {
        private readonly TravelPackageService _travelPackageService;

        public TravelPackageController(TravelPackageService travelPackageService)
        {
            _travelPackageService = travelPackageService;
        }

        [HttpGet]
        public async Task<IActionResult> Get(int skip = 0, int take = 10)
        {
            var packages = await _travelPackageService.GetAllAsync(skip, take);
            return Ok(packages);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var package = await _travelPackageService.GetByIdAsync(id);
            if (package == null)
                return NotFound();

            return Ok(package);
        }

		[HttpPost("Create")]
		public async Task<IActionResult> Create([FromBody] TravelPackage package)
        {
			var createdPackage = await _travelPackageService.CreateAsync(package);
            return CreatedAtAction(nameof(GetById), new { id = createdPackage.TravelPackageId }, createdPackage);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] TravelPackage package)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (id != package.TravelPackageId)
                return BadRequest("ID mismatch.");

            try
            {
                var updatedPackage = await _travelPackageService.UpdateAsync(package);
                return Ok(updatedPackage);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                await _travelPackageService.DeleteAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }
    }
}
