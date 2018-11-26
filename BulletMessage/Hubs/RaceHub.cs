using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BulletMessage.Hubs
{
    public class RaceHub : Hub
    {
        //private static IHubCallerClients Hub { get; set; }
        private ILogger<RaceHub> _logger;
        private readonly IHubContext<ClientHub> _clientHub;

        public RaceHub(ILogger<RaceHub> logger, IHubContext<ClientHub> clientHub)
        {
            _logger = logger;
            _clientHub = clientHub;
        }

        public override async Task OnConnectedAsync()
        {
            await Clients.All.SendAsync("onConnect", $"{Context.ConnectionId} joined");
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            await Clients.All.SendAsync("onDisconnect", $"{Context.ConnectionId} left");
        }

        public async Task SendMessage(string userID, string msg)
        {
            _logger.LogDebug($"{userID}=>{msg}");
            // await Clients.All.SendAsync("ReceiveMessage", userID,msg);
            var avatorUrl = "https://www.pymnts.com/wp-content/uploads/2014/12/green-dot-logo.jpg";

            await Clients.All.SendAsync("setRacer", userID, avatorUrl, msg);
        }

        public async Task SendMessageAll(string msg)
        {
            _logger.LogInformation($"SendMessageAll=>{msg}");
            await _clientHub.Clients.All.SendAsync("ReceiveMessage", "all", msg);
        }

        public async Task FinishRun(string userID, string msg)
        {
            _logger.LogInformation($"FinishRun {userID}=>{msg}");
            // await Clients.All.SendAsync("ReceiveMessage", userID,msg);
            // var avatorUrl = "https://www.pymnts.com/wp-content/uploads/2014/12/green-dot-logo.jpg";

            // await Clients.All.SendAsync("setRacer", userID, avatorUrl, msg);
            var connectionId = ClientHub.GetConnectionID(userID);
            await _clientHub.Clients.Client(connectionId)?.SendAsync("ReceiveMessage", userID, msg);
        }
    }
}
