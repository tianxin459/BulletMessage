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
        public override async Task OnDisconnectedAsync(Exception ex)
        {
            ClientList = ClientList.Where(c => c.ConnectionId != Context.ConnectionId).ToList();
            await Clients.All.SendAsync("ReceiveMessage", $"{Context.ConnectionId} left");
        }


        public Task Register(string userid, string avatorUrl)
        {
            if (ClientList.Any(c => c.UserId == userid))
            {
                ClientList.Where(c => c.UserId == userid).FirstOrDefault().ConnectionId = Context.ConnectionId;
            }
            else
            {
                ClientList.Add(new ClientInfo()
                {
                    UserId = userid,
                    AvatorUrl = avatorUrl,
                    ConnectionId = Context.ConnectionId
                });
            }
            return Clients.Client(Context.ConnectionId).SendAsync("onRegister", $"{Context.ConnectionId}: {userid}");
        }


        public async Task SendMessage(string userID, string msg)
        {
            _logger.LogDebug($"SendMessage {userID}=>{msg}");
            if (userID == "all")
            {
                await Clients.All.SendAsync("ReceiveMessage", userID, msg);
            }
            else
            {
                // var client = Clients.Client(Context.ConnectionId);
                var user = ClientList.FirstOrDefault(c => c.UserId == userID);
                if (user == null) return;
                await Clients.Client(user.ConnectionId)?.SendAsync("ReceiveMessage", userID, msg);
            }
        }
    }
}