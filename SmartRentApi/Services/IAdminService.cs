using System.Collections.Generic;
using System.Threading.Tasks;
using SmartRentApi.DTOs;

namespace SmartRentApi.Services
{
    /// <summary>
    /// Service interface for Admin-related business logic
    /// Handles pending landlord and property approvals
    /// </summary>
    public interface IAdminService
    {
        /// <summary>
        /// Get all landlords with pending approval status
        /// </summary>
        Task<List<UserResponseDto>> GetPendingLandlordsAsync();

        /// <summary>
        /// Get all properties pending approval
        /// </summary>
        Task<List<PropertyResponseDto>> GetPendingPropertiesAsync();

        /// <summary>
        /// Update landlord approval status
        /// </summary>
        Task UpdateLandlordStatusAsync(int userId, SmartRentApi.Models.AccountStatus status);

        /// <summary>
        /// Update property approval status (approve or reject)
        /// </summary>
        Task UpdatePropertyStatusAsync(int propertyId, bool approve);

        /// <summary>
        /// Approve a landlord (legacy endpoint)
        /// </summary>
        Task ApproveLandlordAsync(int userId);

        /// <summary>
        /// Approve a property (legacy endpoint)
        /// </summary>
        Task ApprovePropertyAsync(int propertyId);
    }
}
