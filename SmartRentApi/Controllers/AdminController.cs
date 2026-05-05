using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartRentApi.Data;
using SmartRentApi.DTOs;
using SmartRentApi.Models;
using System.Threading.Tasks;

namespace SmartRentApi.Controllers
{
    [Route("api/admin")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET /api/admin/landlords/pending
        [HttpGet("landlords/pending")]
        public async Task<IActionResult> GetPendingLandlords()
        {
            var landlords = await _context.Users
                .Where(u => u.Role == Role.Landlord && u.AccountStatus == AccountStatus.Pending)
                .Select(u => new UserResponseDto
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    Email = u.Email,
                    Role = u.Role,
                    AccountStatus = u.AccountStatus,
                    CreatedAt = u.CreatedAt
                }).ToListAsync();

            return Ok(landlords);
        }

        // GET /api/admin/properties/pending
        [HttpGet("properties/pending")]
        public async Task<IActionResult> GetPendingProperties()
        {
            var properties = await _context.Properties
                .AsNoTracking()
                .Where(p => p.Status == PropertyStatus.Pending)
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
                    LandlordName = p.Landlord != null ? p.Landlord.FullName : "Unknown landlord"
                }).ToListAsync();

            return Ok(properties);
        }

        // PUT /api/admin/landlords/{id}/status
        // Body: { "status": 1 }  (1 = Approved, 2 = Rejected)
        [HttpPut("landlords/{id}/status")]
        public async Task<IActionResult> UpdateLandlordStatus(int id, [FromBody] AdminStatusUpdateDto dto)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
                return NotFound($"User with ID {id} not found.");

            if (user.Role != Role.Landlord)
                return BadRequest("User is not a landlord.");

            if (user.AccountStatus != AccountStatus.Pending)
                return BadRequest("Landlord account status is not pending.");

            user.AccountStatus = dto.Status;
            await _context.SaveChangesAsync();

            return Ok(new { message = $"Landlord status updated to {dto.Status}." });
        }

        // PUT /api/admin/properties/{id}/status
        // Body: { "approve": true/false }
        [HttpPut("properties/{id}/status")]
        public async Task<IActionResult> UpdatePropertyStatus(int id, [FromBody] AdminPropertyStatusDto dto)
        {
            var property = await _context.Properties.FindAsync(id);

            if (property == null)
                return NotFound($"Property with ID {id} not found.");

            if (property.Status != PropertyStatus.Pending)
                return BadRequest("Property status is not pending.");

            property.Status = dto.Approve ? PropertyStatus.Available : PropertyStatus.Rented; // Rented used as a rejected state, or we add Rejected
            // Actually we set it back or keep pending with a rejection flag - let's use a simpler approach:
            // Approve → Available, Reject → keep as Pending but we return a message? 
            // Better: just remove pending property on reject or mark as rejected.
            // Since enum only has Pending/Available/Rented, rejection will delete the property listing.
            if (!dto.Approve)
            {
                _context.Properties.Remove(property);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Property listing rejected and removed." });
            }

            property.Status = PropertyStatus.Available;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Property approved and is now available." });
        }

        // Keep old approve endpoints for backward compatibility
        [HttpPut("landlords/{id}/approve")]
        public async Task<IActionResult> ApproveLandlord(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();
            if (user.Role != Role.Landlord) return BadRequest("User is not a landlord.");
            if (user.AccountStatus != AccountStatus.Pending) return BadRequest("Not pending.");
            user.AccountStatus = AccountStatus.Approved;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Landlord approved successfully." });
        }

        [HttpPut("properties/{id}/approve")]
        public async Task<IActionResult> ApproveProperty(int id)
        {
            var property = await _context.Properties.FindAsync(id);
            if (property == null) return NotFound();
            if (property.Status != PropertyStatus.Pending) return BadRequest("Not pending.");
            property.Status = PropertyStatus.Available;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Property approved successfully." });
        }
    }
}
