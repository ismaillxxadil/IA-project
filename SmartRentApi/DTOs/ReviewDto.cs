using System;
using System.ComponentModel.DataAnnotations;

namespace SmartRentApi.DTOs
{
    public class ReviewDto
    {
        public int Id { get; set; }
        public int PropertyId { get; set; }
        public string TenantName { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateReviewDto
    {
        public int PropertyId { get; set; }
        [Required, Range(1, 5)]
        public int Rating { get; set; }
        [MaxLength(4000)]
        public string Comment { get; set; }
    }
}
