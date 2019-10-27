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
        public string ActId { get; set; }
        public string UserId { get; set; }
        public string ConnectionId { get; set; }
        public string AvatorUrl { get; set; }
    }
    public class ActInfo
    {
        public string ActId { get; set; }
        public List<ClientInfo> OnViewClientList { get; set; } = new List<ClientInfo>();
    }




    public class ClientHub : Hub
    {
        public static List<ClientInfo> ClientList { get; set; } = new List<ClientInfo>();
        public static List<ActInfo> ActivityList { get; set; } = new List<ActInfo>();

        public static string GetConnectionID(string userId)
        {
            var user = ClientList.FirstOrDefault(c => c.UserId == userId);
            if (user == null) return "";
            return user.ConnectionId;
        }


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
            var client = ClientList.FirstOrDefault() ?? new ClientInfo();
            await Clients.All.SendAsync("ReceiveMessage", $"{Context.ConnectionId}-{client.ActId}-{client.UserId}-left");
        }


        public Task Register(string userID, string avatorUrl)
        {
            if (ClientList.Any(c => c.UserId == userID))
            {
                ClientList.Where(c => c.UserId == userID).FirstOrDefault().ConnectionId = Context.ConnectionId;
            }
            else
            {
                ClientList.Add(new ClientInfo()
                {
                    UserId = userID,
                    AvatorUrl = avatorUrl,
                    ConnectionId = Context.ConnectionId
                });
            }
            return Clients.Client(Context.ConnectionId).SendAsync("onRegister", $"{Context.ConnectionId}: {userID}");
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
                // var user = ClientList.FirstOrDefault(c => c.UserId == userID);
                // if (user == null) return;
                var connectionId = GetConnectionID(userID);
                if (string.IsNullOrEmpty(connectionId)) return;
                await Clients.Client(connectionId)?.SendAsync("ReceiveMessage", userID, msg);
            }
        }


        #region Rger
        public async Task SendToAll(string userID, string msg)
        {
            _logger.LogDebug($"SendMessage {userID}=>{msg}");
            await Clients.All.SendAsync("ReceiveMessage", userID, msg);
        }
        public async Task SendRgerMessage(string actID, string userID, string avatorUrl)
        {
            _logger.LogDebug($"SendMessage {userID}=>{avatorUrl}");
            await Clients.All.SendAsync("ReceiveMessage", actID, userID, avatorUrl);
        }

        public async Task SendRgerMessageQuit(string actID, string userID, string avatorUrl)
        {
            _logger.LogDebug($"SendMessage Quit {userID}=>{avatorUrl}");
            await Clients.All.SendAsync("ReceiveMessageQuit", actID, userID, avatorUrl);
        }

        public Task RegisterRger(string actID, string userID, string avatorUrl)
        {
            if (ClientList.Any(c => c.UserId == userID))
            {
                ClientList.Where(c => c.UserId == userID).FirstOrDefault().ConnectionId = Context.ConnectionId;
                ClientList.Where(c => c.UserId == userID).FirstOrDefault().ActId = actID;
            }
            else
            {
                ClientList.Add(new ClientInfo()
                {
                    ActId = actID,
                    UserId = userID,
                    AvatorUrl = avatorUrl,
                    ConnectionId = Context.ConnectionId
                });
            }


            var act = ActivityList.Where(a => a.ActId == actID)?.FirstOrDefault();
            // clear the act list
            ActivityList
                .ForEach(a => a.OnViewClientList = a.OnViewClientList.Where(c => c.UserId != userID)
                ?.ToList());

            var clientInfo = new ClientInfo()
            {
                ActId = actID,
                UserId = userID,
                AvatorUrl = avatorUrl,
            };
            if (act != null)
            {
                act.OnViewClientList.Add(clientInfo);
            }
            else
            {
                ActivityList.Add(new ActInfo()
                {
                    ActId = actID,
                    OnViewClientList = new List<ClientInfo>() { clientInfo }
                });
            }

            return Clients.All.SendAsync("onRegister", $"{actID}-{userID}-{avatorUrl}");
            // return Clients.Client(Context.ConnectionId).SendAsync("onRegister", $"{actID}-{userID}-{avatorUrl}");
        }
        #endregion Rger

    }
}