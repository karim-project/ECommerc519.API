using Microsoft.AspNetCore.SignalR;

namespace ECommerc519.API.Utitlies
{
    public class ChatHub : Hub
    {
        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }
    }
}
