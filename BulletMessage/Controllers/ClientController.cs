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
using BulletMessage.Contract.Model;
using Newtonsoft.Json;

namespace BulletMessage.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientController : ControllerBase
    {
        private readonly IHubContext<ClientHub> _hubContext;
        private readonly ILogger<ClientController> _logger;

        public static List<User> UserList = null;

        public ClientController(IHubContext<ClientHub> hubContext, ILogger<ClientController> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
            if (UserList == null)
                UserList = readUserListFromStorage();
        }
        public static string ConfigurationJson { get; set; }


        [HttpGet]
        [Route("ping")]
        public IActionResult Ping()
        {
            return Ok("ping");
        }

        private List<User> readUserListFromStorage()
        {
            var path = Directory.GetCurrentDirectory() + @"\userList";
            if (!System.IO.File.Exists(path))
            {
                _logger.LogError(path + " not exists");
                return new List<User>();
            }
            string line;

            if (UserList == null) UserList = new List<User>();
            var outputStr = "";
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    outputStr = sr.ReadToEnd();
                    // while ((line = sr.ReadLine()) != null)
                    // {

                    //     if (line.Split("\t").Count() < 3) _logger.LogError("read UserList Error line=> " + line);
                    //     string nickName = line.Split("\t")[0];
                    //     string englishName = line.Split("\t")[1];
                    //     string avatarUrl = line.Split("\t")[2];

                    //     string model = "";
                    //     string email = "";
                    //     if (line.Split("\t").Count() > 4)
                    //     {
                    //         email = avatarUrl = line.Split("\t")[3];
                    //         model = avatarUrl = line.Split("\t")[4];
                    //     }


                    //     UserList.Add(new User()
                    //     {
                    //         NickName = nickName,
                    //         AvatarUrl = avatarUrl,
                    //         EnglisthName = englishName,
                    //         Email = email,
                    //         Model = model
                    //     });
                    // }
                }
            }
            UserList = JsonConvert.DeserializeObject<List<User>>(outputStr);
            return UserList;
        }

        [HttpGet]
        [Route("getUserList")]
        public IActionResult GetUserList()
        {
            return Ok(UserList);
        }

        [HttpPost]
        [Route("setUser")]
        public IActionResult SetUser(ClientRequest request)
        {
            //ConfigurationJson = request.ConfigurationJson;
            var path = Directory.GetCurrentDirectory() + @"\userList";

            if (UserList.Any(u => u.NickName == request.NickName))
                return Ok(new { Success = false, Message = "duplicate user", req = request });

            using (FileStream fs = new FileStream(path, FileMode.Create))
            {
                StreamWriter sw = new StreamWriter(fs);
                try
                {
                    UserList.Add(new User()
                    {
                        NickName = request.NickName,
                        AvatarUrl = request.AvatarUrl,
                        EnglishName = request.EnglishName,
                        Email = request.Email,
                        Model = request.Model
                    });
                    sw.Write(JsonConvert.SerializeObject(UserList));
                    // sw.WriteLine(request.NickName + "\t" + request.EnglisthName + "\t" + request.AvatarUrl + "\t" + request.Email + "\t" + request.Model);
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