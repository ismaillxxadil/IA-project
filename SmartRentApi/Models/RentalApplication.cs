using System;
using System.ComponentModel.DataAnnotations;

namespace SmartRentApi.Models
{
    public class RentalApplication
    {
        public int Id { get; set; }

        [Required]
        public int PropertyId { get; set; }
        public virtual Property Property { get; set; } = null!;

        [Required]
        public int TenantId { get; set; }
        public virtual User Tenant { get; set; } = null!;

        [Required, MaxLength(1000)]
        public required string DocumentUrl { get; set; }

        [Required]
        public ApplicationStatus Status { get; set; }

        [Required]
        public DateTime AppliedAt { get; set; } = DateTime.UtcNow;
    }
}
