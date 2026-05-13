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
    /// Service class for Admin-related business logic
    /// Manages landlord and property approvals using repositories
    /// </summary>
    public class AdminService : IAdminService
    {
        private readonly IGenericRepository<User> _userRepository;
        private readonly IGenericRepository<Property> _propertyRepository;

        public AdminService(
            IGenericRepository<User> userRepository,
            IGenericRepository<Property> propertyRepository)
        {
            _userRepository = userRepository;
            _propertyRepository = propertyRepository;
        }

        /// <summary>
        /// Get all landlords with pending approval status
        /// </summary>
        public async Task<List<UserResponseDto>> GetPendingLandlordsAsync()
        {
            var landlords = await _userRepository.GetAll()
                .Where(u => u.Role == Role.Landlord && u.AccountStatus == AccountStatus.Pending)
                .Select(u => new UserResponseDto
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    Email = u.Email,
                    Role = u.Role,
                    AccountStatus = u.AccountStatus,
                    CreatedAt = u.CreatedAt
                })
                .ToListAsync();

            return landlords;
        }

        /// <summary>
        /// Get all properties pending approval
        /// </summary>
        public async Task<List<PropertyResponseDto>> GetPendingPropertiesAsync()
        {
            var properties = await _propertyRepository.GetAll()
                .Include(p => p.Landlord)
                .Where(p => p.Status == PropertyStatus.Pending)
                .Select(p => new PropertyResponseDto
                {
                    Id = p.Id,
                    Title = p.Title,
                    Description = p.Description,
                    Price = p.Price,
                    Location = p.Location,
                    Type = p.Type,
                    HasParking = p.HasParking,
                    HasElevator = p.HasElevator,
                    IsFurnished = p.IsFurnished,
                    Status = p.Status,
                    CreatedAt = p.CreatedAt,
                    LandlordId = p.LandlordId,
                    LandlordName = p.Landlord != null ? p.Landlord.FullName : "Unknown landlord"
                })
                .ToListAsync();

            return properties;
        }

        /// <summary>
        /// Update landlord approval status with validation
        /// </summary>
        public async Task UpdateLandlordStatusAsync(int userId, AccountStatus status)
        {
            var user = await _userRepository.GetByIdAsync(userId);

            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID {userId} not found.");
            }

            if (user.Role != Role.Landlord)
            {
                throw new InvalidOperationException("User is not a landlord.");
            }

            if (user.AccountStatus != AccountStatus.Pending)
            {
                throw new InvalidOperationException("Landlord account status is not pending.");
            }

            user.AccountStatus = status;
            _userRepository.Update(user);
            await _userRepository.SaveChangesAsync();
        }

        /// <summary>
        /// Update property approval status (approve or reject)
        /// </summary>
        public async Task UpdatePropertyStatusAsync(int propertyId, bool approve)
        {
            var property = await _propertyRepository.GetByIdAsync(propertyId);

            if (property == null)
            {
                throw new KeyNotFoundException($"Property with ID {propertyId} not found.");
            }

            if (property.Status != PropertyStatus.Pending)
            {
                throw new InvalidOperationException("Property status is not pending.");
            }

            if (!approve)
            {
                // Reject: remove the property listing
                _propertyRepository.Delete(property);
                await _propertyRepository.SaveChangesAsync();
            }
            else
            {
                // Approve: set to available
                property.Status = PropertyStatus.Available;
                _propertyRepository.Update(property);
                await _propertyRepository.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Approve a landlord (legacy endpoint)
        /// </summary>
        public async Task ApproveLandlordAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);

            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID {userId} not found.");
            }

            if (user.Role != Role.Landlord)
            {
                throw new InvalidOperationException("User is not a landlord.");
            }

            if (user.AccountStatus != AccountStatus.Pending)
            {
                throw new InvalidOperationException("User account status is not pending.");
            }

            user.AccountStatus = AccountStatus.Approved;
            _userRepository.Update(user);
            await _userRepository.SaveChangesAsync();
        }

        /// <summary>
        /// Approve a property (legacy endpoint)
        /// </summary>
        public async Task ApprovePropertyAsync(int propertyId)
        {
            var property = await _propertyRepository.GetByIdAsync(propertyId);

            if (property == null)
            {
                throw new KeyNotFoundException($"Property with ID {propertyId} not found.");
            }

            if (property.Status != PropertyStatus.Pending)
            {
                throw new InvalidOperationException("Property status is not pending.");
            }

            property.Status = PropertyStatus.Available;
            _propertyRepository.Update(property);
            await _propertyRepository.SaveChangesAsync();
        }
    }
}
