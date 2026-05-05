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
    public class VisitsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<NotificationHub> _hubContext;

        public VisitsController(ApplicationDbContext context, IHubContext<NotificationHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        [HttpPost]
        [Authorize(Roles = "Tenant")]
        public async Task<IActionResult> RequestVisit([FromBody] CreateVisitRequestDto dto)
        {
            var tenantIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(tenantIdString, out int tenantId)) return Unauthorized();

            var property = await _context.Properties.FindAsync(dto.PropertyId);
            if (property == null || property.Status != PropertyStatus.Available)
                return BadRequest("Property is not available or does not exist.");

            var visitRequest = new VisitRequest
            {
                TenantId = tenantId,
                PropertyId = dto.PropertyId,
                ScheduledDate = dto.ScheduledDate,
                Status = VisitRequestStatus.Pending
            };

            _context.VisitRequests.Add(visitRequest);
            await _context.SaveChangesAsync();

            // Notify Landlord
            await _hubContext.Clients.User(property.LandlordId.ToString())
                .SendAsync("ReceiveNotification", $"New visit request for your property: {property.Title}");

            return Ok(new { message = "Visit request submitted successfully." });
        }

        [HttpGet]
        public async Task<IActionResult> GetVisits()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var role = User.FindFirstValue(ClaimTypes.Role);
            if (!int.TryParse(userIdString, out int userId)) return Unauthorized();

            var query = _context.VisitRequests
                .Include(v => v.Property)
                .Include(v => v.Tenant)
                .AsQueryable();

            if (role == "Tenant")
            {
                query = query.Where(v => v.TenantId == userId);
            }
            else if (role == "Landlord")
            {
                query = query.Where(v => v.Property.LandlordId == userId);
            }
            else
            {
                return Forbid();
            }

            var visits = await query.Select(v => new VisitRequestDto
            {
                Id = v.Id,
                PropertyId = v.PropertyId,
                PropertyTitle = v.Property.Title,
                TenantId = v.TenantId,
                TenantName = v.Tenant.FullName,
                ScheduledDate = v.ScheduledDate,
                Status = v.Status,
                CreatedAt = v.CreatedAt
            }).ToListAsync();

            return Ok(visits);
        }

        [HttpPut("{id}/status")]
        [Authorize(Roles = "Landlord")]
        public async Task<IActionResult> UpdateVisitStatus(int id, [FromBody] UpdateVisitStatusDto dto)
        {
            var landlordIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(landlordIdString, out int landlordId)) return Unauthorized();

            var visit = await _context.VisitRequests
                .Include(v => v.Property)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (visit == null) return NotFound("Visit request not found.");

            if (visit.Property.LandlordId != landlordId)
                return Forbid("You can only update visit requests for your own properties.");

            visit.Status = dto.Status;
            await _context.SaveChangesAsync();

            // Notify Tenant
            await _hubContext.Clients.User(visit.TenantId.ToString())
                .SendAsync("ReceiveNotification", $"Your visit request for {visit.Property.Title} was {dto.Status}.");

            return Ok(new { message = "Visit request status updated." });
        }
    }
}
