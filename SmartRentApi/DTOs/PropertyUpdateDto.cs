using System.ComponentModel.DataAnnotations;
using SmartRentApi.Models;
using System.Collections.Generic;

namespace SmartRentApi.DTOs
{
    public class PropertyUpdateDto
    {
        [Required, MaxLength(200)]
        public required string Title { get; set; }

        [MaxLength(4000)]
        public required string Description { get; set; }

        [Required]
        public decimal Price { get; set; }

        [Required, MaxLength(500)]
        public required string Location { get; set; }

        [Required]
        public PropertyType Type { get; set; }

        public bool HasParking { get; set; }
        
        public bool HasElevator { get; set; }
        
        public bool IsFurnished { get; set; }

        // Optional image URLs to replace existing images
        public List<string> ImageUrls { get; set; } = new List<string>();
    }
}
