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
    /// Service implementation for VisitRequest-related business logic
    /// </summary>
    public class VisitService : IVisitService
    {
        private readonly IGenericRepository<VisitRequest> _visitRepository;
        private readonly IGenericRepository<Property> _propertyRepository;
        private readonly IHubContext<NotificationHub> _hubContext;

        public VisitService(
            IGenericRepository<VisitRequest> visitRepository,
            IGenericRepository<Property> propertyRepository,
            IHubContext<NotificationHub> hubContext)
        {
            _visitRepository = visitRepository;
            _propertyRepository = propertyRepository;
            _hubContext = hubContext;
        }

        public async Task RequestVisitAsync(CreateVisitRequestDto dto, int tenantId)
        {
            // Validate property exists and is available
            var property = await _propertyRepository.GetByIdAsync(dto.PropertyId);
            if (property == null || property.Status != PropertyStatus.Available)
            {
                throw new KeyNotFoundException("Property is not available or does not exist.");
            }

            // Create visit request
            var visitRequest = new VisitRequest
            {
                TenantId = tenantId,
                PropertyId = dto.PropertyId,
                ScheduledDate = dto.ScheduledDate,
                Status = VisitRequestStatus.Pending
            };

            await _visitRepository.AddAsync(visitRequest);
            await _visitRepository.SaveChangesAsync();

            // Notify landlord
            var landlordId = property.LandlordId.ToString();
            await _hubContext.Clients.User(landlordId)
                .SendAsync("ReceiveNotification", $"New visit request for your property: {property.Title}");
        }

        public async Task<List<VisitRequestDto>> GetVisitsAsync(int userId, Role userRole)
        {
            var query = _visitRepository.GetAll()
                .Include(v => v.Property)
                .Include(v => v.Tenant)
                .AsQueryable();

            if (userRole == Role.Tenant)
            {
                query = query.Where(v => v.TenantId == userId);
            }
            else if (userRole == Role.Landlord)
            {
                query = query.Where(v => v.Property.LandlordId == userId);
            }
            else
            {
                throw new UnauthorizedAccessException("Invalid user role.");
            }

            var visits = await query.Select(v => new VisitRequestDto
            {
                Id = v.Id,
                PropertyId = v.PropertyId,
                PropertyTitle = v.Property.Title,
                TenantId = v.TenantId,
                TenantName = v.Tenant.FullName,
                ScheduledDate = v.ScheduledDate,
                Status = v.Status,
                CreatedAt = v.CreatedAt
            }).ToListAsync();

            return visits;
        }

        public async Task UpdateVisitStatusAsync(int visitId, int landlordId, VisitRequestStatus newStatus)
        {
            // Get visit with related data
            var visit = await _visitRepository.GetAll()
                .Include(v => v.Property)
                .FirstOrDefaultAsync(v => v.Id == visitId);

            if (visit == null)
            {
                throw new KeyNotFoundException("Visit request not found.");
            }

            // Verify landlord owns the property
            if (visit.Property.LandlordId != landlordId)
            {
                throw new UnauthorizedAccessException("You can only update visit requests for your own properties.");
            }

            visit.Status = newStatus;
            _visitRepository.Update(visit);
            await _visitRepository.SaveChangesAsync();

            // Notify tenant
            var tenantId = visit.TenantId.ToString();
            await _hubContext.Clients.User(tenantId)
                .SendAsync("ReceiveNotification", $"Your visit request for {visit.Property.Title} was {newStatus}.");
        }
    }
}
