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
        private ILogger<ClientHub> _logger;

        public ClientHub(ILogger<ClientHub> logger)
        {
            _logger = logger;
        }
    }
}