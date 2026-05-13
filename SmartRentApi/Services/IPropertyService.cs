using System.Collections.Generic;
using System.Threading.Tasks;
using SmartRentApi.DTOs;

namespace SmartRentApi.Services
{
    /// <summary>
    /// Service interface for Property-related business logic
    /// Encapsulates all property operations including validation and data access
    /// </summary>
    public interface IPropertyService
    {
        /// <summary>
        /// Create a new property with associated images
        /// </summary>
        Task<PropertyResponseDto> CreatePropertyAsync(PropertyCreateDto dto);

        /// <summary>
        /// Get all available properties with optional filtering
        /// </summary>
        Task<List<PropertyResponseDto>> GetAvailablePropertiesAsync(
            string? location = null, 
            decimal? minPrice = null, 
            decimal? maxPrice = null, 
            SmartRentApi.Models.PropertyType? type = null);

        /// <summary>
        /// Get a single property by ID
        /// </summary>
        Task<PropertyResponseDto?> GetPropertyByIdAsync(int id);

        /// <summary>
        /// Get all properties owned by a specific landlord
        /// </summary>
        Task<List<PropertyResponseDto>> GetPropertiesByLandlordAsync(int landlordId);

        /// <summary>
        /// Update an existing property
        /// </summary>
        Task<PropertyResponseDto> UpdatePropertyAsync(int id, int landlordId, SmartRentApi.DTOs.PropertyUpdateDto dto);

        /// <summary>
        /// Delete a property owned by a landlord
        /// </summary>
        Task DeletePropertyAsync(int id, int landlordId);
    }
}
