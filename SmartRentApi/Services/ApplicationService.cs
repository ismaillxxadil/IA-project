using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SmartRentApi.DTOs;
using SmartRentApi.Hubs;
using SmartRentApi.Models;
using SmartRentApi.Repositories;

namespace SmartRentApi.Services
{
    /// <summary>
    /// Service implementation for RentalApplication-related business logic
    /// </summary>
    public class ApplicationService : IApplicationService
    {
        private readonly IGenericRepository<RentalApplication> _applicationRepository;
        private readonly IGenericRepository<Property> _propertyRepository;
        private readonly IGenericRepository<User> _userRepository;
        private readonly IHubContext<NotificationHub> _hubContext;

        public ApplicationService(
            IGenericRepository<RentalApplication> applicationRepository,
            IGenericRepository<Property> propertyRepository,
            IGenericRepository<User> userRepository,
            IHubContext<NotificationHub> hubContext)
        {
            _applicationRepository = applicationRepository;
            _propertyRepository = propertyRepository;
            _userRepository = userRepository;
            _hubContext = hubContext;
        }

        public async Task SubmitApplicationAsync(CreateRentalApplicationDto dto, int tenantId)
        {
            // Validate property exists and is available
            var property = await _propertyRepository.GetByIdAsync(dto.PropertyId);
            if (property == null || property.Status != PropertyStatus.Available)
            {
                throw new KeyNotFoundException("Property is not available for rent or does not exist.");
            }

            // Check for existing pending application
            var existingApplication = await _applicationRepository.GetAll()
                .AnyAsync(a => a.TenantId == tenantId && 
                    a.PropertyId == dto.PropertyId && 
                    a.Status == ApplicationStatus.Pending);

            if (existingApplication)
            {
                throw new InvalidOperationException("You already have a pending application for this property.");
            }

            // Create application
            var application = new RentalApplication
            {
                TenantId = tenantId,
                PropertyId = dto.PropertyId,
                DocumentUrl = dto.DocumentUrl,
                Status = ApplicationStatus.Pending
            };

            await _applicationRepository.AddAsync(application);
            await _applicationRepository.SaveChangesAsync();

            // Notify landlord
            var landlordId = property.LandlordId.ToString();
            await _hubContext.Clients.User(landlordId)
                .SendAsync("ReceiveNotification", $"New rental application for your property: {property.Title}");
        }

        public async Task<List<RentalApplicationDto>> GetApplicationsAsync(int userId, Role userRole)
        {
            var query = _applicationRepository.GetAll()
                .Include(a => a.Property)
                .Include(a => a.Tenant)
                .AsQueryable();

            if (userRole == Role.Tenant)
            {
                query = query.Where(a => a.TenantId == userId);
            }
            else if (userRole == Role.Landlord)
            {
                query = query.Where(a => a.Property.LandlordId == userId);
            }
            else
            {
                throw new UnauthorizedAccessException("Invalid user role.");
            }

            var applications = await query.Select(a => new RentalApplicationDto
            {
                Id = a.Id,
                PropertyId = a.PropertyId,
                PropertyTitle = a.Property.Title,
                TenantId = a.TenantId,
                TenantName = a.Tenant.FullName,
                DocumentUrl = a.DocumentUrl,
                Status = a.Status,
                AppliedAt = a.AppliedAt
            }).ToListAsync();

            return applications;
        }

        public async Task UpdateApplicationStatusAsync(int applicationId, int landlordId, ApplicationStatus newStatus)
        {
            // Get application with related data
            var application = await _applicationRepository.GetAll()
                .Include(a => a.Property)
                .FirstOrDefaultAsync(a => a.Id == applicationId);

            if (application == null)
            {
                throw new KeyNotFoundException("Rental application not found.");
            }

            // Verify landlord owns the property
            if (application.Property.LandlordId != landlordId)
            {
                throw new UnauthorizedAccessException("You can only manage applications for your own properties.");
            }

            application.Status = newStatus;

            // If accepted, mark property as rented and reject other pending applications
            if (newStatus == ApplicationStatus.Accepted)
            {
                application.Property.Status = PropertyStatus.Rented;

                var otherPending = await _applicationRepository.GetAll()
                    .Where(a => a.PropertyId == application.PropertyId && 
                        a.Id != applicationId && 
                        a.Status == ApplicationStatus.Pending)
                    .ToListAsync();

                foreach (var otherApp in otherPending)
                {
                    otherApp.Status = ApplicationStatus.Rejected;
                }
            }

            _applicationRepository.Update(application);
            await _applicationRepository.SaveChangesAsync();

            // Notify tenant
            var tenantId = application.TenantId.ToString();
            await _hubContext.Clients.User(tenantId)
                .SendAsync("ReceiveNotification", $"Your rental application for {application.Property.Title} was {newStatus}.");
        }
    }
}
