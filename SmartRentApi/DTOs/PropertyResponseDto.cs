using System;
using System.Collections.Generic;
using SmartRentApi.Models;

namespace SmartRentApi.DTOs
{
    public class PropertyResponseDto
    {
        public int Id { get; set; }
        public required string Title { get; set; }
        public required string Description { get; set; }
        public decimal Price { get; set; }
        public required string Location { get; set; }
        public PropertyType Type { get; set; }
        public bool HasParking { get; set; }
        public bool HasElevator { get; set; }
        public bool IsFurnished { get; set; }
        public PropertyStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        
        public required string LandlordName { get; set; }
        public int LandlordId { get; set; }

        public List<string> ImageUrls { get; set; } = new List<string>();
    }
}
