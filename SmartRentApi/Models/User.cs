using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SmartRentApi.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public required string FullName { get; set; }

        [Required, EmailAddress, MaxLength(200)]
        public required string Email { get; set; }

        [Required, MaxLength(500)]
        public required string PasswordHash { get; set; }

        public Role Role { get; set; }

        public AccountStatus AccountStatus { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual ICollection<Property> Properties { get; set; } = new HashSet<Property>();
        public virtual ICollection<VisitRequest> VisitRequests { get; set; } = new HashSet<VisitRequest>();
        public virtual ICollection<RentalApplication> RentalApplications { get; set; } = new HashSet<RentalApplication>();
        public virtual ICollection<Favorite> Favorites { get; set; } = new HashSet<Favorite>();
        public virtual ICollection<Review> Reviews { get; set; } = new HashSet<Review>();
    }
}
