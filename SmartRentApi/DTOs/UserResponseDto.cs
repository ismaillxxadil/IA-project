using System;
using SmartRentApi.Models;

namespace SmartRentApi.DTOs
{
    public class UserResponseDto
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public Role Role { get; set; }
        public AccountStatus AccountStatus { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
