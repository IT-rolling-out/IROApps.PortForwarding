using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace IROApps.PortForwarding.ServerApp.Service
{
    public class ChatHub : Hub
    {
        public override Task OnConnectedAsync()
        {
            return base.OnConnectedAsync();
        }

        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }
    }
}