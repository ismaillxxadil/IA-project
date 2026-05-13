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
    /// Service class for Property-related business logic
    /// Handles all property operations and validation using repositories
    /// </summary>
    public class PropertyService : IPropertyService
    {
        private readonly IGenericRepository<Property> _propertyRepository;
        private readonly IGenericRepository<User> _userRepository;
        private readonly IGenericRepository<PropertyImage> _propertyImageRepository;

        public PropertyService(
            IGenericRepository<Property> propertyRepository,
            IGenericRepository<User> userRepository,
            IGenericRepository<PropertyImage> propertyImageRepository)
        {
            _propertyRepository = propertyRepository;
            _userRepository = userRepository;
            _propertyImageRepository = propertyImageRepository;
        }

        /// <summary>
        /// Create a new property with validation and image associations
        /// </summary>
        public async Task<PropertyResponseDto> CreatePropertyAsync(PropertyCreateDto dto)
        {
            // Validate landlord exists and is approved
            var landlord = await _userRepository.GetByIdAsync(dto.LandlordId);
            if (landlord == null || landlord.Role != Role.Landlord)
            {
                throw new ArgumentException("Invalid Landlord.");
            }

            if (landlord.AccountStatus != AccountStatus.Approved)
            {
                throw new ArgumentException("Landlord account must be approved to add properties.");
            }

            // Create the property
            var property = new Property
            {
                LandlordId = dto.LandlordId,
                Title = dto.Title,
                Description = dto.Description,
                Price = dto.Price,
                Location = dto.Location,
                Type = dto.Type,
                HasParking = dto.HasParking,
                HasElevator = dto.HasElevator,
                IsFurnished = dto.IsFurnished,
                Status = PropertyStatus.Pending
            };

            await _propertyRepository.AddAsync(property);
            await _propertyRepository.SaveChangesAsync();

            // Add images if provided
            if (dto.ImageUrls != null && dto.ImageUrls.Any())
            {
                foreach (var url in dto.ImageUrls.Where(u => !string.IsNullOrWhiteSpace(u)))
                {
                    var image = new PropertyImage
                    {
                        PropertyId = property.Id,
                        ImageUrl = url.Trim()
                    };
                    await _propertyImageRepository.AddAsync(image);
                }
                await _propertyImageRepository.SaveChangesAsync();
            }

            // Return the created property as DTO
            return await MapPropertyToResponseDtoAsync(property, landlord);
        }

        /// <summary>
        /// Get all available properties with optional filters
        /// </summary>
        public async Task<List<PropertyResponseDto>> GetAvailablePropertiesAsync(
            string? location = null,
            decimal? minPrice = null,
            decimal? maxPrice = null,
            PropertyType? type = null)
        {
            var query = _propertyRepository.GetAll()
                .Include(p => p.Landlord)
                .Include(p => p.Images)
                .Where(p => p.Status == PropertyStatus.Available)
                .AsQueryable();

            if (!string.IsNullOrEmpty(location))
            {
                query = query.Where(p => p.Location.Contains(location));
            }

            if (minPrice.HasValue)
            {
                query = query.Where(p => p.Price >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(p => p.Price <= maxPrice.Value);
            }

            if (type.HasValue)
            {
                query = query.Where(p => p.Type == type.Value);
            }

            var properties = await query.ToListAsync();

            var result = new List<PropertyResponseDto>();
            foreach (var property in properties)
            {
                var dto = MapPropertyToResponseDto(property);
                result.Add(dto);
            }

            return result;
        }

        /// <summary>
        /// Get a single property by ID
        /// </summary>
        public async Task<PropertyResponseDto?> GetPropertyByIdAsync(int id)
        {
            var property = await _propertyRepository.GetAll()
                .Include(p => p.Landlord)
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (property == null)
            {
                return null;
            }

            return MapPropertyToResponseDto(property);
        }

        /// <summary>
        /// Get all properties owned by a specific landlord
        /// </summary>
        public async Task<List<PropertyResponseDto>> GetPropertiesByLandlordAsync(int landlordId)
        {
            var properties = await _propertyRepository.GetAll()
                .Include(p => p.Landlord)
                .Include(p => p.Images)
                .Where(p => p.LandlordId == landlordId)
                .ToListAsync();

            return properties.Select(p => MapPropertyToResponseDto(p)).ToList();
        }

        /// <summary>
        /// Update a property (only by its owner landlord)
        /// </summary>
        public async Task<PropertyResponseDto> UpdatePropertyAsync(int id, int landlordId, PropertyUpdateDto dto)
        {
            var property = await _propertyRepository.GetByIdAsync(id);
            if (property == null)
            {
                throw new KeyNotFoundException($"Property with ID {id} not found.");
            }

            if (property.LandlordId != landlordId)
            {
                throw new UnauthorizedAccessException("You can only update your own properties.");
            }

            // Update property fields
            property.Title = dto.Title;
            property.Description = dto.Description;
            property.Price = dto.Price;
            property.Location = dto.Location;
            property.Type = dto.Type;
            property.HasParking = dto.HasParking;
            property.HasElevator = dto.HasElevator;
            property.IsFurnished = dto.IsFurnished;

            _propertyRepository.Update(property);
            await _propertyRepository.SaveChangesAsync();

            // Update images: remove old, add new
            var existingImages = await _propertyImageRepository.GetAll()
                .Where(pi => pi.PropertyId == id)
                .ToListAsync();

            foreach (var image in existingImages)
            {
                _propertyImageRepository.Delete(image);
            }

            if (dto.ImageUrls != null && dto.ImageUrls.Any())
            {
                foreach (var url in dto.ImageUrls.Where(u => !string.IsNullOrWhiteSpace(u)))
                {
                    var image = new PropertyImage
                    {
                        PropertyId = property.Id,
                        ImageUrl = url.Trim()
                    };
                    await _propertyImageRepository.AddAsync(image);
                }
            }

            await _propertyImageRepository.SaveChangesAsync();

            // Reload to get updated images
            var updatedProperty = await _propertyRepository.GetAll()
                .Include(p => p.Landlord)
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == id);

            return MapPropertyToResponseDto(updatedProperty!);
        }

        /// <summary>
        /// Delete a property (only by its owner landlord)
        /// </summary>
        public async Task DeletePropertyAsync(int id, int landlordId)
        {
            var property = await _propertyRepository.GetByIdAsync(id);
            if (property == null)
            {
                throw new KeyNotFoundException($"Property with ID {id} not found.");
            }

            if (property.LandlordId != landlordId)
            {
                throw new UnauthorizedAccessException("You can only delete your own properties.");
            }

            _propertyRepository.Delete(property);
            await _propertyRepository.SaveChangesAsync();
        }

        /// <summary>
        /// Helper method: Map Property entity to PropertyResponseDto
        /// </summary>
        private PropertyResponseDto MapPropertyToResponseDto(Property property)
        {
            return new PropertyResponseDto
            {
                Id = property.Id,
                Title = property.Title,
                Description = property.Description,
                Price = property.Price,
                Location = property.Location,
                Type = property.Type,
                HasParking = property.HasParking,
                HasElevator = property.HasElevator,
                IsFurnished = property.IsFurnished,
                Status = property.Status,
                CreatedAt = property.CreatedAt,
                LandlordId = property.LandlordId,
                LandlordName = property.Landlord?.FullName ?? "Unknown",
                ImageUrls = property.Images?.Select(i => i.ImageUrl).ToList() ?? new List<string>()
            };
        }

        /// <summary>
        /// Helper method: Map Property entity to PropertyResponseDto asynchronously
        /// </summary>
        private async Task<PropertyResponseDto> MapPropertyToResponseDtoAsync(Property property, User landlord)
        {
            var images = await _propertyImageRepository.GetAll()
                .Where(pi => pi.PropertyId == property.Id)
                .Select(pi => pi.ImageUrl)
                .ToListAsync();

            return new PropertyResponseDto
            {
                Id = property.Id,
                Title = property.Title,
                Description = property.Description,
                Price = property.Price,
                Location = property.Location,
                Type = property.Type,
                HasParking = property.HasParking,
                HasElevator = property.HasElevator,
                IsFurnished = property.IsFurnished,
                Status = property.Status,
                CreatedAt = property.CreatedAt,
                LandlordId = landlord.Id,
                LandlordName = landlord.FullName,
                ImageUrls = images
            };
        }
    }
}
