using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BulletMessage.Hubs
{
    public class RaceHub:Hub
    {
        //private static IHubCallerClients Hub { get; set; }
        private ILogger<RaceHub> _logger;

        public RaceHub(ILogger<RaceHub> logger)
        {
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            await Clients.All.SendAsync("onConnect", $"{Context.ConnectionId} joined");
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            await Clients.All.SendAsync("onDisconnect", $"{Context.ConnectionId} left");
        }

        public async Task SendMessage(string userID,string msg)
        {
            _logger.LogInformation($"{userID}=>{msg}");
            await Clients.All.SendAsync("ReceiveMessage", userID,msg);
        }
    }
}
