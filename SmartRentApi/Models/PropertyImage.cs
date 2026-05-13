using System.ComponentModel.DataAnnotations;

namespace SmartRentApi.Models
{
    public class PropertyImage
    {
        public int Id { get; set; }

        [Required]
        public int PropertyId { get; set; }
        public virtual Property Property { get; set; } = null!;

        [Required, MaxLength(1000)]
        public required string ImageUrl { get; set; }
    }
}
