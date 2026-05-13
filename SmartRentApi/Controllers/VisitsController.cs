using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartRentApi.DTOs;
using SmartRentApi.Models;
using SmartRentApi.Services;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SmartRentApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class VisitsController : ControllerBase
    {
        private readonly IVisitService _visitService;

        public VisitsController(IVisitService visitService)
        {
            _visitService = visitService;
        }

        [HttpPost]
        [Authorize(Roles = "Tenant")]
        public async Task<IActionResult> RequestVisit([FromBody] CreateVisitRequestDto dto)
        {
            var tenantIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(tenantIdString, out int tenantId)) return Unauthorized();

            try
            {
                await _visitService.RequestVisitAsync(dto, tenantId);
                return Ok(new { message = "Visit request submitted successfully." });
            }
            catch (KeyNotFoundException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetVisits()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var role = User.FindFirstValue(ClaimTypes.Role);
            if (!int.TryParse(userIdString, out int userId)) return Unauthorized();

            try
            {
                if (!Enum.TryParse<Role>(role, out var userRole))
                {
                    return Forbid();
                }

                var visits = await _visitService.GetVisitsAsync(userId, userRole);
                return Ok(visits);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        [HttpPut("{id}/status")]
        [Authorize(Roles = "Landlord")]
        public async Task<IActionResult> UpdateVisitStatus(int id, [FromBody] UpdateVisitStatusDto dto)
        {
            var landlordIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(landlordIdString, out int landlordId)) return Unauthorized();

            try
            {
                await _visitService.UpdateVisitStatusAsync(id, landlordId, dto.Status);
                return Ok(new { message = "Visit request status updated." });
            }
            catch (KeyNotFoundException)
            {
                return NotFound("Visit request not found.");
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
        }
    }
}
