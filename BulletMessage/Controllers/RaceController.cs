using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
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
        [Route("rungroup")]
        public IActionResult RunGroup(RaceRequest request)
        {
            _logger.LogInformation(JsonConvert.SerializeObject(request));
            HttpContext.Request.Headers.TryGetValue("eventId", out var headerEventId);
            HttpContext.Request.Headers.TryGetValue("groupId", out var headerGroupId);
            var eventId = headerEventId.ToString();
            var groupId = headerGroupId.ToString();
            var step = 0;
            int.TryParse(request.Message, out step);
            if (step != 0 && request.Gender == 2)
            {
                request.Message = (step * 1).ToString();
            }
            _hubContext.Clients.All.SendAsync("setRacer", groupId, request.AvatorUrl, request.Message, request.EnglishName);
            return Ok(new { Success = true });
        }

        [HttpPost]
        [Route("run")]
        public IActionResult Run(RaceRequest request)
        {
            _logger.LogInformation(JsonConvert.SerializeObject(request));
            HttpContext.Request.Headers.TryGetValue("eventId", out var headerEventId);
            HttpContext.Request.Headers.TryGetValue("groupId", out var headerGroupId);
            var eventId = headerEventId.ToString();
            var groupId = headerGroupId.ToString();
            var step = 0;
            int.TryParse(request.Message, out step);
            if (step != 0 && request.Gender == 2)
            {
                request.Message = (step * 1).ToString();
            }
            _hubContext.Clients.All.SendAsync("setRacer", request.UserId, request.AvatorUrl, request.Message, request.EnglishName);
            return Ok(new { Success = true });
        }


        [HttpPost]
        [Route("runevent")]
        public IActionResult RunForEvent(RaceRequest request)
        {
            _logger.LogInformation(JsonConvert.SerializeObject(request));
            HttpContext.Request.Headers.TryGetValue("eventId", out var headerEventId);
            HttpContext.Request.Headers.TryGetValue("groupId", out var headerGroupId);
            var eventId = headerEventId.ToString();
            var groupId = headerGroupId.ToString();
            var step = 0;
            int.TryParse(request.Message, out step);
            if (step != 0 && request.Gender == 2)
            {
                request.Message = (step * 1).ToString();
            }
            _hubContext.Clients.All.SendAsync("setRacer", request.UserId, request.AvatorUrl, request.Message, request.EnglishName);
            return Ok(new { Success = true });
        }

        [HttpPost]
        [Route("ready")]
        public IActionResult Ready(RaceRequest request)
        {
            _logger.LogDebug(JsonConvert.SerializeObject(request));
            _hubContext.Clients.All.SendAsync("readyRacer", request.UserId, request.AvatorUrl, request.Message);
            return Ok(new { Success = true });
        }



        [HttpPost]
        [Route("begin")]
        public IActionResult Begin(RaceRequest request)
        {
            _logger.LogDebug(JsonConvert.SerializeObject(request));
            if (request.Message == "GoGoGo")
            {
                _hubContext.Clients.All.SendAsync("beginRace", request.Message);
            }
            return Ok(new { Success = true });
        }


        [HttpPost]
        [Route("result")]
        public IActionResult Result(RaceResultRequest request)
        {
            _logger.LogError(request.Value);
            var strDateTime = DateTime.Now.ToString("yyMMddhhmmssfff"); //取得时间字符串
            var path = Directory.GetCurrentDirectory() + @"\raceresult_" + strDateTime;
            using (FileStream fs = new FileStream(path, FileMode.Create))
            {
                StreamWriter sw = new StreamWriter(fs);
                try
                {
                    sw.Write(request.Value);
                    // sw.WriteLine(request.NickName + "\t" + request.EnglisthName + "\t" + request.AvatarUrl + "\t" + request.Email + "\t" + request.Model);
                }
                catch (Exception e)
                {
                    _logger.LogError(e.Message);
                    throw e;
                }
                finally
                {
                    sw.Flush();
                    sw.Close();
                    fs.Close();
                }
            }
            return Ok(new { Success = true });
        }
    }
}