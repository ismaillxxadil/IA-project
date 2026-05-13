using System.Collections.Generic;
using System.Threading.Tasks;
using SmartRentApi.DTOs;
using SmartRentApi.Models;

namespace SmartRentApi.Services
{
    /// <summary>
    /// Service interface for RentalApplication-related business logic
    /// </summary>
    public interface IApplicationService
    {
        /// <summary>
        /// Submit a rental application for a property
        /// </summary>
        Task SubmitApplicationAsync(CreateRentalApplicationDto dto, int tenantId);

        /// <summary>
        /// Get rental applications filtered by user role (tenant or landlord)
        /// </summary>
        Task<List<RentalApplicationDto>> GetApplicationsAsync(int userId, Role userRole);

        /// <summary>
        /// Update application status with cascading effects
        /// </summary>
        Task UpdateApplicationStatusAsync(int applicationId, int landlordId, ApplicationStatus newStatus);
    }
}
