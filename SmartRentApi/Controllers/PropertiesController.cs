using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartRentApi.Data;
using SmartRentApi.DTOs;
using SmartRentApi.Models;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SmartRentApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PropertiesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PropertiesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        [Authorize(Roles = "Landlord")]
        public async Task<IActionResult> CreateProperty([FromBody] PropertyCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var landlord = await _context.Users.FindAsync(dto.LandlordId);
            if (landlord == null || landlord.Role != Role.Landlord)
            {
                return BadRequest("Invalid Landlord.");
            }

            if (landlord.AccountStatus != AccountStatus.Approved)
            {
                return BadRequest("Landlord account must be approved to add properties.");
            }

            var property = new Property
            {
                LandlordId = dto.LandlordId,
                Title = dto.Title,
                Description = dto.Description,
                Price = dto.Price,
                Location = dto.Location,
                Type = dto.Type,
                HasParking = dto.HasParking,
                HasElevator = dto.HasElevator,
                IsFurnished = dto.IsFurnished,
                Status = PropertyStatus.Pending
            };

            _context.Properties.Add(property);
            await _context.SaveChangesAsync();

            // Save optional images
            if (dto.ImageUrls != null && dto.ImageUrls.Any())
            {
                foreach (var url in dto.ImageUrls.Where(u => !string.IsNullOrWhiteSpace(u)))
                {
                    _context.PropertyImages.Add(new PropertyImage { PropertyId = property.Id, ImageUrl = url.Trim() });
                }
                await _context.SaveChangesAsync();
            }

            var responseDto = new PropertyResponseDto
            {
                Id = property.Id,
                Title = property.Title,
                Description = property.Description,
                Price = property.Price,
                Location = property.Location,
                Type = property.Type,
                HasParking = property.HasParking,
                HasElevator = property.HasElevator,
                IsFurnished = property.IsFurnished,
                Status = property.Status,
                CreatedAt = property.CreatedAt,
                LandlordId = landlord.Id,
                LandlordName = landlord.FullName,
                ImageUrls = await _context.PropertyImages
                    .Where(pi => pi.PropertyId == property.Id)
                    .Select(pi => pi.ImageUrl)
                    .ToListAsync()
            };

            return CreatedAtAction(nameof(GetPropertyById), new { id = property.Id }, responseDto);
        }

        [HttpGet]
        public async Task<IActionResult> GetProperties([FromQuery] string? location, [FromQuery] decimal? minPrice, [FromQuery] decimal? maxPrice, [FromQuery] PropertyType? type)
        {
            var query = _context.Properties
                .Include(p => p.Landlord)
                .Include(p => p.Images)
                .Where(p => p.Status == PropertyStatus.Available)
                .AsQueryable();

            if (!string.IsNullOrEmpty(location))
            {
                query = query.Where(p => p.Location.Contains(location));
            }

            if (minPrice.HasValue)
            {
                query = query.Where(p => p.Price >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(p => p.Price <= maxPrice.Value);
            }

            if (type.HasValue)
            {
                query = query.Where(p => p.Type == type.Value);
            }

            var properties = await query.Select(p => new PropertyResponseDto
            {
                Id = p.Id,
                Title = p.Title,
                Description = p.Description,
                Price = p.Price,
                Location = p.Location,
                Type = p.Type,
                HasParking = p.HasParking,
                HasElevator = p.HasElevator,
                IsFurnished = p.IsFurnished,
                Status = p.Status,
                CreatedAt = p.CreatedAt,
                LandlordId = p.LandlordId,
                LandlordName = p.Landlord.FullName,
                ImageUrls = p.Images.Select(i => i.ImageUrl).ToList()
            }).ToListAsync();

            return Ok(properties);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPropertyById(int id)
        {
            var property = await _context.Properties
                .Include(p => p.Landlord)
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (property == null)
            {
                return NotFound($"Property with ID {id} not found.");
            }

            var responseDto = new PropertyResponseDto
            {
                Id = property.Id,
                Title = property.Title,
                Description = property.Description,
                Price = property.Price,
                Location = property.Location,
                Type = property.Type,
                HasParking = property.HasParking,
                HasElevator = property.HasElevator,
                IsFurnished = property.IsFurnished,
                Status = property.Status,
                CreatedAt = property.CreatedAt,
                LandlordId = property.LandlordId,
                LandlordName = property.Landlord.FullName,
                ImageUrls = property.Images.Select(i => i.ImageUrl).ToList()
            };

            return Ok(responseDto);
        }

        [HttpGet("my-properties")]
        [Authorize(Roles = "Landlord")]
        public async Task<IActionResult> GetMyProperties()
        {
            var landlordIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(landlordIdString, out int landlordId)) return Unauthorized();

            var properties = await _context.Properties
                .Include(p => p.Landlord)
                .Include(p => p.Images)
                .Where(p => p.LandlordId == landlordId)
                .Select(p => new PropertyResponseDto
                {
                    Id = p.Id,
                    Title = p.Title,
                    Description = p.Description,
                    Price = p.Price,
                    Location = p.Location,
                    Type = p.Type,
                    HasParking = p.HasParking,
                    HasElevator = p.HasElevator,
                    IsFurnished = p.IsFurnished,
                    Status = p.Status,
                    CreatedAt = p.CreatedAt,
                    LandlordId = p.LandlordId,
                    LandlordName = p.Landlord.FullName,
                    ImageUrls = p.Images.Select(i => i.ImageUrl).ToList()
                }).ToListAsync();

            return Ok(properties);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Landlord")]
        public async Task<IActionResult> UpdateProperty(int id, [FromBody] PropertyUpdateDto dto)
        {
            var landlordIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(landlordIdString, out int landlordId)) return Unauthorized();

            var property = await _context.Properties.FindAsync(id);
            if (property == null) return NotFound("Property not found.");

            if (property.LandlordId != landlordId)
                return Forbid("You can only update your own properties.");

            property.Title = dto.Title;
            property.Description = dto.Description;
            property.Price = dto.Price;
            property.Location = dto.Location;
            property.Type = dto.Type;
            property.HasParking = dto.HasParking;
            property.HasElevator = dto.HasElevator;
            property.IsFurnished = dto.IsFurnished;

            // Update images: replace existing with provided list
            var existing = _context.PropertyImages.Where(pi => pi.PropertyId == property.Id);
            _context.PropertyImages.RemoveRange(existing);
            if (dto.ImageUrls != null && dto.ImageUrls.Any())
            {
                foreach (var url in dto.ImageUrls.Where(u => !string.IsNullOrWhiteSpace(u)))
                {
                    _context.PropertyImages.Add(new PropertyImage { PropertyId = property.Id, ImageUrl = url.Trim() });
                }
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = "Property updated successfully." });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Landlord")]
        public async Task<IActionResult> DeleteProperty(int id)
        {
            var landlordIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(landlordIdString, out int landlordId)) return Unauthorized();

            var property = await _context.Properties.FindAsync(id);
            if (property == null) return NotFound("Property not found.");

            if (property.LandlordId != landlordId)
                return Forbid("You can only delete your own properties.");

            _context.Properties.Remove(property);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Property deleted successfully." });
        }
    }
}
