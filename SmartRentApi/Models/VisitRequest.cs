using System;
using System.ComponentModel.DataAnnotations;

namespace SmartRentApi.Models
{
    public class VisitRequest
    {
        public int Id { get; set; }

        [Required]
        public int PropertyId { get; set; }
        public virtual Property Property { get; set; } = null!;

        [Required]
        public int TenantId { get; set; }
        public virtual User Tenant { get; set; } = null!;

        [Required]
        public DateTime ScheduledDate { get; set; }

        [Required]
        public VisitRequestStatus Status { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
