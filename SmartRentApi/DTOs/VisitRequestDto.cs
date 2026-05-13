using System;
using SmartRentApi.Models;

namespace SmartRentApi.DTOs
{
    public class VisitRequestDto
    {
        public int Id { get; set; }
        public int PropertyId { get; set; }
        public required string PropertyTitle { get; set; }
        public int TenantId { get; set; }
        public required string TenantName { get; set; }
        public DateTime ScheduledDate { get; set; }
        public VisitRequestStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateVisitRequestDto
    {
        public int PropertyId { get; set; }
        public DateTime ScheduledDate { get; set; }
    }

    public class UpdateVisitStatusDto
    {
        public VisitRequestStatus Status { get; set; }
    }
}
