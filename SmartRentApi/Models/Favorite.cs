using System;
using System.ComponentModel.DataAnnotations;

namespace SmartRentApi.Models
{
    public class Favorite
    {
        public int Id { get; set; }

        [Required]
        public int TenantId { get; set; }
        public virtual User Tenant { get; set; } = null!;

        [Required]
        public int PropertyId { get; set; }
        public virtual Property Property { get; set; } = null!;

        [Required]
        public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    }
}
