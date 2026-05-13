using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartRentApi.Models
{
    public class Property
    {
        public int Id { get; set; }

        [Required]
        public int LandlordId { get; set; }
        public virtual User Landlord { get; set; } = null!;

        [Required, MaxLength(200)]
        public required string Title { get; set; }

        [MaxLength(4000)]
        public required string Description { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [Required, MaxLength(500)]
        public required string Location { get; set; }

        [Required]
        public PropertyType Type { get; set; }

        public bool HasParking { get; set; }

        public bool HasElevator { get; set; }

        public bool IsFurnished { get; set; }

        [Required]
        public PropertyStatus Status { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        //this line do : add Images, VisitRequests, RentalApplications, Favorites, Reviews in table Property as relation between tables   
        public virtual ICollection<PropertyImage> Images { get; set; } = new HashSet<PropertyImage>();
        public virtual ICollection<VisitRequest> VisitRequests { get; set; } = new HashSet<VisitRequest>();
        public virtual ICollection<RentalApplication> RentalApplications { get; set; } = new HashSet<RentalApplication>();
        public virtual ICollection<Favorite> Favorites { get; set; } = new HashSet<Favorite>();
        public virtual ICollection<Review> Reviews { get; set; } = new HashSet<Review>();
    }
}
