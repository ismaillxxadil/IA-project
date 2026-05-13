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
    public class ApplicationsController : ControllerBase
    {
        private readonly IApplicationService _applicationService;

        public ApplicationsController(IApplicationService applicationService)
        {
            _applicationService = applicationService;
        }

        [HttpPost]
        [Authorize(Roles = "Tenant")]
        public async Task<IActionResult> SubmitApplication([FromBody] CreateRentalApplicationDto dto)
        {
            var tenantIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(tenantIdString, out int tenantId)) return Unauthorized();

            try
            {
                await _applicationService.SubmitApplicationAsync(dto, tenantId);
                return Ok(new { message = "Rental application submitted successfully." });
            }
            catch (KeyNotFoundException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetApplications()
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

                var applications = await _applicationService.GetApplicationsAsync(userId, userRole);
                return Ok(applications);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        [HttpPut("{id}/status")]
        [Authorize(Roles = "Landlord")]
        public async Task<IActionResult> UpdateApplicationStatus(int id, [FromBody] UpdateApplicationStatusDto dto)
        {
            var landlordIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(landlordIdString, out int landlordId)) return Unauthorized();

            try
            {
                await _applicationService.UpdateApplicationStatusAsync(id, landlordId, dto.Status);
                return Ok(new { message = $"Application status updated to {dto.Status}." });
            }
            catch (KeyNotFoundException)
            {
                return NotFound("Rental application not found.");
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
        }
    }
}
