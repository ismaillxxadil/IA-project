using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartRentApi.DTOs;
using SmartRentApi.Models;
using SmartRentApi.Services;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SmartRentApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PropertiesController : ControllerBase
    {
        private readonly IPropertyService _propertyService;

        public PropertiesController(IPropertyService propertyService)
        {
            // Dependency injection -> property service 
            _propertyService = propertyService;
        }

        [HttpPost]
        [Authorize(Roles = "Landlord")]
        //post api/properties
        public async Task<IActionResult> CreateProperty([FromBody] PropertyCreateDto dto)
        {
            
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var responseDto = await _propertyService.CreatePropertyAsync(dto);
                return CreatedAtAction(nameof(GetPropertyById), new { id = responseDto.Id }, responseDto);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetProperties([FromQuery] string? location, [FromQuery] decimal? minPrice, [FromQuery] decimal? maxPrice, [FromQuery] PropertyType? type)
        {
            var properties = await _propertyService.GetAvailablePropertiesAsync(location, minPrice, maxPrice, type);
            return Ok(properties);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPropertyById(int id)
        {
            var property = await _propertyService.GetPropertyByIdAsync(id);
            if (property == null)
            {
                return NotFound($"Property with ID {id} not found.");
            }
            return Ok(property);
        }

        [HttpGet("my-properties")]
        [Authorize(Roles = "Landlord")]
        public async Task<IActionResult> GetMyProperties()
        {
            var landlordIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(landlordIdString, out int landlordId)) return Unauthorized();

            var properties = await _propertyService.GetPropertiesByLandlordAsync(landlordId);
            return Ok(properties);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Landlord")]
        public async Task<IActionResult> UpdateProperty(int id, [FromBody] PropertyUpdateDto dto)
        {
            var landlordIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(landlordIdString, out int landlordId)) return Unauthorized();

            try
            {
                var updated = await _propertyService.UpdatePropertyAsync(id, landlordId, dto);
                return Ok(new { message = "Property updated successfully.", data = updated });
            }
            catch (KeyNotFoundException)
            {
                return NotFound("Property not found.");
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid("You can only update your own properties.");
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Landlord")]
        public async Task<IActionResult> DeleteProperty(int id)
        {
            var landlordIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(landlordIdString, out int landlordId)) return Unauthorized();

            try
            {
                await _propertyService.DeletePropertyAsync(id, landlordId);
                return Ok(new { message = "Property deleted successfully." });
            }
            catch (KeyNotFoundException)
            {
                return NotFound("Property not found.");
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid("You can only delete your own properties.");
            }
        }
    }
}
