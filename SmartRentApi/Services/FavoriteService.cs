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
    /// Service implementation for Favorite-related business logic
    /// </summary>
    public class FavoriteService : IFavoriteService
    {
        private readonly IGenericRepository<Favorite> _favoriteRepository;
        private readonly IGenericRepository<Property> _propertyRepository;

        public FavoriteService(
            IGenericRepository<Favorite> favoriteRepository,
            IGenericRepository<Property> propertyRepository)
        {
            _favoriteRepository = favoriteRepository;
            _propertyRepository = propertyRepository;
        }

        public async Task AddFavoriteAsync(CreateFavoriteDto dto, int tenantId)
        {
            // Validate property exists and is available
            var property = await _propertyRepository.GetByIdAsync(dto.PropertyId);
            if (property == null || property.Status != PropertyStatus.Available)
            {
                throw new KeyNotFoundException("Property is not available or does not exist.");
            }

            // Check if already favorited
            var exists = await _favoriteRepository.GetAll()
                .AnyAsync(f => f.TenantId == tenantId && f.PropertyId == dto.PropertyId);

            if (exists)
            {
                throw new InvalidOperationException("Property is already in favorites.");
            }

            // Add to favorites
            var favorite = new Favorite
            {
                TenantId = tenantId,
                PropertyId = dto.PropertyId
            };

            await _favoriteRepository.AddAsync(favorite);
            await _favoriteRepository.SaveChangesAsync();
        }

        public async Task<List<FavoriteDto>> GetFavoritesAsync(int tenantId)
        {
            var favorites = await _favoriteRepository.GetAll()
                .Include(f => f.Property)
                .Where(f => f.TenantId == tenantId)
                .Select(f => new FavoriteDto
                {
                    Id = f.Id,
                    PropertyId = f.PropertyId,
                    PropertyTitle = f.Property.Title,
                    AddedAt = f.AddedAt
                })
                .ToListAsync();

            return favorites;
        }

        public async Task RemoveFavoriteAsync(int propertyId, int tenantId)
        {
            var favorite = await _favoriteRepository.GetAll()
                .FirstOrDefaultAsync(f => f.TenantId == tenantId && f.PropertyId == propertyId);

            if (favorite == null)
            {
                throw new KeyNotFoundException("Favorite not found.");
            }

            _favoriteRepository.Delete(favorite);
            await _favoriteRepository.SaveChangesAsync();
        }
    }
}
