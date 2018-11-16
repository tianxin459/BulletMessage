using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace BulletMessage.Hubs
{
    public class ClientInfo
    {
        public string UserId { get; set; }
        public string ConnectionId { get; set; }
        public string AvatorUrl { get; set; }
    }


    public class ClientHub : Hub
    {
        public static List<ClientInfo> ClientList { get; set; } = new List<ClientInfo>();
        private ILogger<ClientHub> _logger;

        public ClientHub(ILogger<ClientHub> logger)
        {
            _logger = logger;
        }


        public override async Task OnConnectedAsync()
        {
            await Clients.Client(Context.ConnectionId).SendAsync("onConnect", Context.ConnectionId);
            _logger.LogInformation("Context.ConnectionId connected");
        }


        public Task Register(string userid,string avatorUrl)
        {
            ClientList.Add(new ClientInfo()
            {
                UserId = userid,
                AvatorUrl = avatorUrl,
                ConnectionId = Context.ConnectionId
            });
            return Clients.Client(Context.ConnectionId).SendAsync("onRegister", $"{Context.ConnectionId}: {userid}");
        }


        public async Task SendMessage(string userID, string msg)
        {
            _logger.LogDebug($"{userID}=>{msg}");
            
            await Clients.All.SendAsync("ReceiveMessage", userID, msg);
        }
    }
}