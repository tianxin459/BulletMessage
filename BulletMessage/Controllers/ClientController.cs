using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BulletMessage.Contract.Request;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BulletMessage.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientController : ControllerBase
    {
        public static string ConfigurationJson { get; set; }
        [HttpGet]
        [Route("config")]
        public IActionResult GetConfig()
        {
            var path = Directory.GetCurrentDirectory() + @"\config";
            StreamReader sr = new StreamReader(path);
            List<string> strOutput = new List<string>();
            string line;
            while ((line = sr.ReadLine()) != null)
            {
                Console.WriteLine(line);
                strOutput.Add(line);
            }
            return Ok();
        }


        [HttpPost]
        [Route("config")]
        public IActionResult SetConfig(ConfigureRequest request)
        {
            ConfigurationJson = request.ConfigurationJson;
            var path = Directory.GetCurrentDirectory()+@"\config";

            FileStream fs = new FileStream(path, FileMode.Create);
            StreamWriter sw = new StreamWriter(fs);
            sw.Write(request.ConfigurationJson);
            sw.Flush();
            sw.Close();
            fs.Close();
            return Ok();
        }
    }
}