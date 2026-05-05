using System.ComponentModel.DataAnnotations;
using SmartRentApi.Models;

namespace SmartRentApi.DTOs
{
    public class RegisterDto
    {
        [Required, MaxLength(200)]
        public string FullName { get; set; }

        [Required, EmailAddress, MaxLength(200)]
        public string Email { get; set; }

        [Required, MinLength(6)]
        public string Password { get; set; }

        [Required]
        public Role Role { get; set; }
    }
}
