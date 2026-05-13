using System.Collections.Generic;
using System.Threading.Tasks;
using SmartRentApi.DTOs;
using SmartRentApi.Models;

namespace SmartRentApi.Services
{
    /// <summary>
    /// Service interface for VisitRequest-related business logic
    /// </summary>
    public interface IVisitService
    {
        /// <summary>
        /// Request a visit for a property
        /// </summary>
        Task RequestVisitAsync(CreateVisitRequestDto dto, int tenantId);

        /// <summary>
        /// Get visit requests filtered by user role (tenant or landlord)
        /// </summary>
        Task<List<VisitRequestDto>> GetVisitsAsync(int userId, Role userRole);

        /// <summary>
        /// Update visit request status
        /// </summary>
        Task UpdateVisitStatusAsync(int visitId, int landlordId, VisitRequestStatus newStatus);
    }
}
