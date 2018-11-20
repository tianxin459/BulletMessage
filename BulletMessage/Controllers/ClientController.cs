using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BulletMessage.Contract.Request;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using BulletMessage.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace BulletMessage.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientController : ControllerBase
    {
        private readonly IHubContext<ClientHub> _hubContext;
        private readonly ILogger<ClientController> _logger;

        public ClientController(IHubContext<ClientHub> hubContext, ILogger<ClientController> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }
        public static string ConfigurationJson { get; set; }




        [HttpGet]
        [Route("ping")]
        public IActionResult Ping()
        {
            return Ok("ping");
        }


        [HttpGet]
        [Route("config")]
        public IActionResult GetConfig()
        {
            var path = Directory.GetCurrentDirectory() + @"\config";
            if (!System.IO.File.Exists(path)) return Ok("no configuration");
            string outputStr;

            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    outputStr = sr.ReadToEnd();
                }
            }
            // using (StreamReader sr = new StreamReader(path))
            // {
            //     outputStr = sr.ReadToEnd();
            // }
            return Ok(outputStr);
        }


        [HttpPost]
        [Route("config")]
        public IActionResult SetConfig(ConfigureRequest request)
        {
            ConfigurationJson = request.ConfigurationJson;
            var path = Directory.GetCurrentDirectory() + @"\config";

            using (FileStream fs = new FileStream(path, FileMode.Create))
            {
                StreamWriter sw = new StreamWriter(fs);
                try
                {
                    sw.Write(request.ConfigurationJson);
                }
                catch (Exception e)
                {
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