using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BulletMessage.Contract.Request;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace BulletMessage.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BulletController : ControllerBase
    {
        private readonly IHubContext<ChatHub> _hubContext;

        public BulletController(IHubContext<ChatHub> hubContext)
        {
            _hubContext = hubContext;
        }
        [HttpGet]
        [Route("ping")]
        public IActionResult get()
        {
            //var hubManager = new DefaultHubManager(GlobalHost.DependencyResolver);
            //var hub = hubManager.ResolveHub("myHub") as ChatHub;
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