using System.Collections.Generic;
using System.Threading.Tasks;
using SmartRentApi.DTOs;

namespace SmartRentApi.Services
{
    /// <summary>
    /// Service interface for Review-related business logic
    /// </summary>
    public interface IReviewService
    {
        /// <summary>
        /// Add a review for a property (tenant must have successfully rented it)
        /// </summary>
        Task AddReviewAsync(CreateReviewDto dto, int tenantId);

        /// <summary>
        /// Get all reviews for a specific property
        /// </summary>
        Task<List<ReviewDto>> GetPropertyReviewsAsync(int propertyId);
    }
}
