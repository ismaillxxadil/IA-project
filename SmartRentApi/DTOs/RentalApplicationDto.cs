using System;
using System.ComponentModel.DataAnnotations;
using SmartRentApi.Models;

namespace SmartRentApi.DTOs
{
    public class RentalApplicationDto
    {
        public int Id { get; set; }
        public int PropertyId { get; set; }
        public required string PropertyTitle { get; set; }
        public int TenantId { get; set; }
        public required string TenantName { get; set; }
        public required string DocumentUrl { get; set; }
        public ApplicationStatus Status { get; set; }
        public DateTime AppliedAt { get; set; }
    }

    public class CreateRentalApplicationDto
    {
        public int PropertyId { get; set; }
        [Required]
        public required string DocumentUrl { get; set; }
    }

    public class UpdateApplicationStatusDto
    {
        public ApplicationStatus Status { get; set; }
    }
}
