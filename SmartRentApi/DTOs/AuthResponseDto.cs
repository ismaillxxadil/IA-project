namespace SmartRentApi.DTOs
{
    public class AuthResponseDto
    {
        public required string Token { get; set; }
        public required UserResponseDto User { get; set; }
    }
}
