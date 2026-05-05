using System;
using System.ComponentModel.DataAnnotations;
using SmartRentApi.Models;

namespace SmartRentApi.DTOs
{
    public class RentalApplicationDto
    {
        public int Id { get; set; }
        public int PropertyId { get; set; }
        public string PropertyTitle { get; set; }
        public int TenantId { get; set; }
        public string TenantName { get; set; }
        public string DocumentUrl { get; set; }
        public ApplicationStatus Status { get; set; }
        public DateTime AppliedAt { get; set; }
    }

    public class CreateRentalApplicationDto
    {
        public int PropertyId { get; set; }
        [Required]
        public string DocumentUrl { get; set; }
    }

    public class UpdateApplicationStatusDto
    {
        public ApplicationStatus Status { get; set; }
    }
}
