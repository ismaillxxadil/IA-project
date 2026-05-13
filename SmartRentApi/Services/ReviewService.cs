using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SmartRentApi.DTOs;
using SmartRentApi.Models;
using SmartRentApi.Repositories;

namespace SmartRentApi.Services
{
    /// <summary>
    /// Service implementation for Review-related business logic
    /// </summary>
    public class ReviewService : IReviewService
    {
        private readonly IGenericRepository<Review> _reviewRepository;
        private readonly IGenericRepository<RentalApplication> _applicationRepository;

        public ReviewService(
            IGenericRepository<Review> reviewRepository,
            IGenericRepository<RentalApplication> applicationRepository)
        {
            _reviewRepository = reviewRepository;
            _applicationRepository = applicationRepository;
        }

        public async Task AddReviewAsync(CreateReviewDto dto, int tenantId)
        {
            // Validate tenant has successfully rented this property
            var hasRented = await _applicationRepository.GetAll()
                .AnyAsync(a => a.TenantId == tenantId && 
                    a.PropertyId == dto.PropertyId && 
                    a.Status == ApplicationStatus.Accepted);

            if (!hasRented)
            {
                throw new InvalidOperationException("You can only review properties you have successfully rented.");
            }

            // Check if review already exists
            var existingReview = await _reviewRepository.GetAll()
                .AnyAsync(r => r.TenantId == tenantId && r.PropertyId == dto.PropertyId);

            if (existingReview)
            {
                throw new InvalidOperationException("You have already reviewed this property.");
            }

            // Create and save review
            var review = new Review
            {
                TenantId = tenantId,
                PropertyId = dto.PropertyId,
                Rating = dto.Rating,
                Comment = dto.Comment
            };

            await _reviewRepository.AddAsync(review);
            await _reviewRepository.SaveChangesAsync();
        }

        public async Task<List<ReviewDto>> GetPropertyReviewsAsync(int propertyId)
        {
            var reviews = await _reviewRepository.GetAll()
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
                })
                .ToListAsync();

            return reviews;
        }
    }
}
