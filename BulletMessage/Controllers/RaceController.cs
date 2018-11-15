﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BulletMessage.Contract.Request;
using BulletMessage.Hubs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace BulletMessage.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RaceController : ControllerBase
    {
        public static List<string> RunnerList = new List<string>();
        private readonly IHubContext<RaceHub> _hubContext;

        private ILogger<RaceController> _logger;
        public RaceController(IHubContext<RaceHub> hubContext, ILogger<RaceController> logger)
        {
            _logger = logger;
            _hubContext = hubContext;
        }

        [HttpPost]
        [Route("run")]
        public IActionResult Run(RaceRequest request)
        {
            _logger.LogInformation(JsonConvert.SerializeObject(request));
            //if(RunnerList.Contains(request.UserId))
            //{
            //    _hubContext.Clients.All.SendAsync("ReceiveMessage", request.UserId, request.Message);
            //}
            //else
            //{
            //    RunnerList.Add(request.UserId);
            //    _hubContext.Clients.All.SendAsync("setRacer", request.UserId, request.AvatorUrl, request.Message);
            //}
            _hubContext.Clients.All.SendAsync("setRacer", request.UserId, request.AvatorUrl, request.Message);
            return Ok(new { Success = true });
        }
    }
}