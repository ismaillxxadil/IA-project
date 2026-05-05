using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace SmartRentApi.Hubs
{
    [Authorize]
    public class NotificationHub : Hub
    {
        // SignalR automatically uses the ClaimTypes.NameIdentifier from the JWT token 
        // to identify users. This allows us to send messages directly to specific User IDs.
        
        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(System.Exception exception)
        {
            await base.OnDisconnectedAsync(exception);
        }
    }
}
