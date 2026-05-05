using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartRentApi.Data;
using SmartRentApi.DTOs;
using SmartRentApi.Models;
using SmartRentApi.Hubs;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace SmartRentApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ApplicationsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<NotificationHub> _hubContext;

        public ApplicationsController(ApplicationDbContext context, IHubContext<NotificationHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        [HttpPost]
        [Authorize(Roles = "Tenant")]
        public async Task<IActionResult> SubmitApplication([FromBody] CreateRentalApplicationDto dto)
        {
            var tenantIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(tenantIdString, out int tenantId)) return Unauthorized();

            var property = await _context.Properties.FindAsync(dto.PropertyId);
            if (property == null || property.Status != PropertyStatus.Available)
                return BadRequest("Property is not available for rent or does not exist.");

            var existingApplication = await _context.RentalApplications
                .AnyAsync(a => a.TenantId == tenantId && a.PropertyId == dto.PropertyId && a.Status == ApplicationStatus.Pending);
            
            if (existingApplication)
                return BadRequest("You already have a pending application for this property.");

            var application = new RentalApplication
            {
                TenantId = tenantId,
                PropertyId = dto.PropertyId,
                DocumentUrl = dto.DocumentUrl,
                Status = ApplicationStatus.Pending
            };

            _context.RentalApplications.Add(application);
            await _context.SaveChangesAsync();

            // Notify Landlord
            await _hubContext.Clients.User(property.LandlordId.ToString())
                .SendAsync("ReceiveNotification", $"New rental application for your property: {property.Title}");

            return Ok(new { message = "Rental application submitted successfully." });
        }

        [HttpGet]
        public async Task<IActionResult> GetApplications()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var role = User.FindFirstValue(ClaimTypes.Role);
            if (!int.TryParse(userIdString, out int userId)) return Unauthorized();

            var query = _context.RentalApplications
                .Include(a => a.Property)
                .Include(a => a.Tenant)
                .AsQueryable();

            if (role == "Tenant")
            {
                query = query.Where(a => a.TenantId == userId);
            }
            else if (role == "Landlord")
            {
                query = query.Where(a => a.Property.LandlordId == userId);
            }
            else
            {
                return Forbid();
            }

            var applications = await query.Select(a => new RentalApplicationDto
            {
                Id = a.Id,
                PropertyId = a.PropertyId,
                PropertyTitle = a.Property.Title,
                TenantId = a.TenantId,
                TenantName = a.Tenant.FullName,
                DocumentUrl = a.DocumentUrl,
                Status = a.Status,
                AppliedAt = a.AppliedAt
            }).ToListAsync();

            return Ok(applications);
        }

        [HttpPut("{id}/status")]
        [Authorize(Roles = "Landlord")]
        public async Task<IActionResult> UpdateApplicationStatus(int id, [FromBody] UpdateApplicationStatusDto dto)
        {
            var landlordIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(landlordIdString, out int landlordId)) return Unauthorized();

            var application = await _context.RentalApplications
                .Include(a => a.Property)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (application == null) return NotFound("Rental application not found.");

            if (application.Property.LandlordId != landlordId)
                return Forbid("You can only manage applications for your own properties.");

            application.Status = dto.Status;

            // If the application is accepted, mark the property as Rented
            if (dto.Status == ApplicationStatus.Accepted)
            {
                application.Property.Status = PropertyStatus.Rented;
                
                // Optionally, we could reject all other pending applications for this property here
                var otherPending = await _context.RentalApplications
                    .Where(a => a.PropertyId == application.PropertyId && a.Id != id && a.Status == ApplicationStatus.Pending)
                    .ToListAsync();
                foreach (var otherApp in otherPending)
                {
                    otherApp.Status = ApplicationStatus.Rejected;
                }
            }

            await _context.SaveChangesAsync();

            // Notify Tenant
            await _hubContext.Clients.User(application.TenantId.ToString())
                .SendAsync("ReceiveNotification", $"Your rental application for {application.Property.Title} was {dto.Status}.");

            return Ok(new { message = $"Application status updated to {dto.Status}." });
        }
    }
}
