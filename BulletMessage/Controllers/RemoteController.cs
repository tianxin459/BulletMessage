using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BulletMessage.Contract.Request;
using BulletMessage.Hubs;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace BulletMessage.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RemoteController : ControllerBase
    {
        private readonly ILogger<RemoteController> _logger;
        private readonly IHubContext<ComHub> _remoteContext;
        private IHostingEnvironment _environment;


        public RemoteController(IHubContext<ComHub> comContext, ILogger<RemoteController> logger, IHostingEnvironment hostingEnvironment)
        {
            _environment = hostingEnvironment;
            _logger = logger;
            _remoteContext = comContext;
        }
        
        [HttpPost]
        [Route("Command")]
        public IActionResult SendCommand(CommandRequest request)
        {
            _logger.LogDebug($"Send Command ReceiveMessage=>{request.Id}:{request.Message}");
            _remoteContext.Clients.All.SendAsync("Command", request.Id, request.Message);

            return Ok(new { success = true,id = request.Id, message = request.Message });
        }
    }
}
