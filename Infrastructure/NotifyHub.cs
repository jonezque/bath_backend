using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

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