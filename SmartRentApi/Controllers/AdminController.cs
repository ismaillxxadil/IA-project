using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartRentApi.DTOs;
using SmartRentApi.Services;
using System;
using System.Threading.Tasks;

namespace SmartRentApi.Controllers
{
    [Route("api/admin")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        /// <summary>
        /// GET /api/admin/landlords/pending
        /// Get all landlords with pending approval status
        /// </summary>
        [HttpGet("landlords/pending")]
        public async Task<IActionResult> GetPendingLandlords()
        {
            var landlords = await _adminService.GetPendingLandlordsAsync();
            return Ok(landlords);
        }

        /// <summary>
        /// GET /api/admin/properties/pending
        /// Get all properties pending approval
        /// </summary>
        [HttpGet("properties/pending")]
        public async Task<IActionResult> GetPendingProperties()
        {
            var properties = await _adminService.GetPendingPropertiesAsync();
            return Ok(properties);
        }

        /// <summary>
        /// PUT /api/admin/landlords/{id}/status
        /// Update landlord approval status
        /// Body: { "status": 1 }  (1 = Approved, 2 = Rejected)
        /// </summary>
        [HttpPut("landlords/{id}/status")]
        public async Task<IActionResult> UpdateLandlordStatus(int id, [FromBody] AdminStatusUpdateDto dto)
        {
            try
            {
                await _adminService.UpdateLandlordStatusAsync(id, dto.Status);
                return Ok(new { message = $"Landlord status updated to {dto.Status}." });
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"User with ID {id} not found.");
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// PUT /api/admin/properties/{id}/status
        /// Update property approval status
        /// Body: { "approve": true/false }
        /// </summary>
        [HttpPut("properties/{id}/status")]
        public async Task<IActionResult> UpdatePropertyStatus(int id, [FromBody] AdminPropertyStatusDto dto)
        {
            try
            {
                await _adminService.UpdatePropertyStatusAsync(id, dto.Approve);
                
                if (dto.Approve)
                {
                    return Ok(new { message = "Property approved and is now available." });
                }
                else
                {
                    return Ok(new { message = "Property listing rejected and removed." });
                }
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Property with ID {id} not found.");
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// PUT /api/admin/landlords/{id}/approve
        /// Approve a landlord (legacy endpoint - kept for backward compatibility)
        /// </summary>
        [HttpPut("landlords/{id}/approve")]
        public async Task<IActionResult> ApproveLandlord(int id)
        {
            try
            {
                await _adminService.ApproveLandlordAsync(id);
                return Ok(new { message = "Landlord approved successfully." });
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// PUT /api/admin/properties/{id}/approve
        /// Approve a property (legacy endpoint - kept for backward compatibility)
        /// </summary>
        [HttpPut("properties/{id}/approve")]
        public async Task<IActionResult> ApproveProperty(int id)
        {
            try
            {
                await _adminService.ApprovePropertyAsync(id);
                return Ok(new { message = "Property approved successfully." });
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
