using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BulletMessage.Contract.Request;
using BulletMessage.Hubs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace BulletMessage.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BulletController : ControllerBase
    {
        private readonly IHubContext<BulletHub> _hubContext;
        private readonly ILogger<BulletController> _logger;

        public BulletController(IHubContext<BulletHub> hubContext, ILogger<BulletController> logger)
        {
            _logger = logger;
            _hubContext = hubContext;
        }
        [HttpGet]
        [Route("ping")]
        public IActionResult get()
        {
            _hubContext.Clients.All.SendAsync("ReceiveMessage", "Admin", "Test" + DateTime.Now.ToLongDateString());
            return Ok("test");
        }


        // POST api/values
        [HttpPost]
        public IActionResult Post(BulletRequest request)
        {
            _hubContext.Clients.All.SendAsync("ReceiveMessage", request.Id, request.Message);
            return Ok(new { success=true });
        }
    }
}