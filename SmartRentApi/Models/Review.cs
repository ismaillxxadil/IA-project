using System;
using System.ComponentModel.DataAnnotations;

namespace SmartRentApi.Models
{
    public class Review
    {
        public int Id { get; set; }

        [Required]
        public int PropertyId { get; set; }
        public virtual Property Property { get; set; }

        [Required]
        public int TenantId { get; set; }
        public virtual User Tenant { get; set; }

        [Required, Range(1, 5)]
        public int Rating { get; set; }

        [MaxLength(4000)]
        public string Comment { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
