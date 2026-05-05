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
    public class ReviewsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ReviewsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        [Authorize(Roles = "Tenant")]
        public async Task<IActionResult> AddReview([FromBody] CreateReviewDto dto)
        {
            var tenantIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(tenantIdString, out int tenantId)) return Unauthorized();

            // Validate that the tenant actually rented this property
            var hasRented = await _context.RentalApplications.AnyAsync(a => 
                a.TenantId == tenantId && 
                a.PropertyId == dto.PropertyId && 
                a.Status == ApplicationStatus.Accepted);

            if (!hasRented)
            {
                return BadRequest("You can only review properties you have successfully rented.");
            }

            var existingReview = await _context.Reviews.AnyAsync(r => r.TenantId == tenantId && r.PropertyId == dto.PropertyId);
            if (existingReview)
            {
                return BadRequest("You have already reviewed this property.");
            }

            var review = new Review
            {
                TenantId = tenantId,
                PropertyId = dto.PropertyId,
                Rating = dto.Rating,
                Comment = dto.Comment
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Review added successfully." });
        }

        [HttpGet("property/{propertyId}")]
        public async Task<IActionResult> GetPropertyReviews(int propertyId)
        {
            var reviews = await _context.Reviews
                .Include(r => r.Tenant)
                .Where(r => r.PropertyId == propertyId)
                .Select(r => new ReviewDto
                {
                    Id = r.Id,
                    PropertyId = r.PropertyId,
                    TenantName = r.Tenant.FullName,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    CreatedAt = r.CreatedAt
                }).ToListAsync();

            return Ok(reviews);
        }
    }
}
