using System;

namespace SmartRentApi.DTOs
{
    public class FavoriteDto
    {
        public int Id { get; set; }
        public int PropertyId { get; set; }
        public required string PropertyTitle { get; set; }
        public DateTime AddedAt { get; set; }
    }

    public class CreateFavoriteDto
    {
        public int PropertyId { get; set; }
    }
}
