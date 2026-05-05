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
    [Authorize(Roles = "Tenant")]
    public class FavoritesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public FavoritesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> AddFavorite([FromBody] CreateFavoriteDto dto)
        {
            var tenantIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(tenantIdString, out int tenantId)) return Unauthorized();

            var property = await _context.Properties.FindAsync(dto.PropertyId);
            if (property == null || property.Status != PropertyStatus.Available)
                return BadRequest("Property is not available or does not exist.");

            var exists = await _context.Favorites.AnyAsync(f => f.TenantId == tenantId && f.PropertyId == dto.PropertyId);
            if (exists) return BadRequest("Property is already in favorites.");

            var favorite = new Favorite
            {
                TenantId = tenantId,
                PropertyId = dto.PropertyId
            };

            _context.Favorites.Add(favorite);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Added to favorites." });
        }

        [HttpGet]
        public async Task<IActionResult> GetFavorites()
        {
            var tenantIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(tenantIdString, out int tenantId)) return Unauthorized();

            var favorites = await _context.Favorites
                .Include(f => f.Property)
                .Where(f => f.TenantId == tenantId)
                .Select(f => new FavoriteDto
                {
                    Id = f.Id,
                    PropertyId = f.PropertyId,
                    PropertyTitle = f.Property.Title,
                    AddedAt = f.AddedAt
                }).ToListAsync();

            return Ok(favorites);
        }

        [HttpDelete("{propertyId}")]
        public async Task<IActionResult> RemoveFavorite(int propertyId)
        {
            var tenantIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(tenantIdString, out int tenantId)) return Unauthorized();

            var favorite = await _context.Favorites.FirstOrDefaultAsync(f => f.TenantId == tenantId && f.PropertyId == propertyId);
            if (favorite == null) return NotFound("Favorite not found.");

            _context.Favorites.Remove(favorite);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Removed from favorites." });
        }
    }
}
