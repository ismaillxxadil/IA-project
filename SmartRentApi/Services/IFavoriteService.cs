using System.Collections.Generic;
using System.Threading.Tasks;
using SmartRentApi.DTOs;

namespace SmartRentApi.Services
{
    /// <summary>
    /// Service interface for Favorite-related business logic
    /// </summary>
    public interface IFavoriteService
    {
        /// <summary>
        /// Add a property to favorites
        /// </summary>
        Task AddFavoriteAsync(CreateFavoriteDto dto, int tenantId);

        /// <summary>
        /// Get all favorites for a tenant
        /// </summary>
        Task<List<FavoriteDto>> GetFavoritesAsync(int tenantId);

        /// <summary>
        /// Remove a property from favorites
        /// </summary>
        Task RemoveFavoriteAsync(int propertyId, int tenantId);
    }
}
