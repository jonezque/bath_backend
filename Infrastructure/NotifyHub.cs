using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace api.Infrastructure
{
    public class NotifyHub : Hub
    {
        public async Task Send(string message)
        {
            await Clients.All.SendAsync("notify", message);
        }
    }
}
