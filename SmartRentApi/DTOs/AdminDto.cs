using SmartRentApi.Models;

namespace SmartRentApi.DTOs
{
    public class AdminStatusUpdateDto
    {
        public AccountStatus Status { get; set; }
    }

    public class AdminPropertyStatusDto
    {
        public bool Approve { get; set; }
    }
}
