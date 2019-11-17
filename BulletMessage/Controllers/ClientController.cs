using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using BulletMessage.Contract.Request;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using BulletMessage.Hubs;
using Microsoft.AspNetCore.SignalR;
using BulletMessage.Contract.Model;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Caching.Memory;

namespace BulletMessage.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientController : ControllerBase
    {
        private readonly IHubContext<ClientHub> _hubContext;
        private readonly ILogger<ClientController> _logger;
        private readonly IConfiguration _configuration;
        private HttpClient _httpClient;
        private IMemoryCache _cache;

        public static List<User> UserList = null;

        public ClientController(IMemoryCache cache, IConfiguration configuration, IHubContext<ClientHub> hubContext, ILogger<ClientController> logger)
        {
            _hubContext = hubContext;
            _configuration = configuration;
            _logger = logger;
            _cache = cache;
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
        [Route("qyaccesstoken")]
        public async Task<string> GetQYAccessToken()
        {
            string accessToken;
            if(_cache.TryGetValue(Common.CK_QYACCESSTOKEN,out accessToken))
            {
                return accessToken;
            }
            else
            {

                var corpid = _configuration.GetValue<string>("wx:corpid");
                var corpsec = _configuration.GetValue<string>("wx:corpsec");
                var url = string.Format("https://qyapi.weixin.qq.com/cgi-bin/gettoken?corpid={0}&corpsecret={1}", corpid, corpsec);
                var responseCode = "";
                // dynamic respObj = new ExpendoObject();
                var respStr = "";
                try
                {
                    if (_httpClient == null) _httpClient = new HttpClient();
                    var resp = await _httpClient.GetAsync(url);
                    respStr = resp.Content.ReadAsStringAsync().Result;
                    dynamic respObj = JsonConvert.DeserializeObject(respStr);
                    if (respObj.access_token == null)
                        accessToken = "error";
                    else
                    {
                        accessToken = respObj.access_token;
                        var expTime = DateTime.Now.AddSeconds(6000);
                        _cache.Set<string>(Common.CK_QYACCESSTOKEN, accessToken, expTime);
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError(e, respStr);
                    dynamic respObj = JsonConvert.DeserializeObject(respStr);
                    responseCode = respObj.errmsg;
                }
            }
            return accessToken;
        }

        [HttpGet]
        [Route("wxqylogin/{jscode}")]
        public async Task<IActionResult> QYLogin(string jscode)
        {
            var accessToken = await GetQYAccessToken();
            var url = string.Format("https://qyapi.weixin.qq.com/cgi-bin/miniprogram/jscode2session?access_token={1}&js_code={0}&grant_type=authorization_code", jscode, accessToken);
            var response = "";
            // dynamic respObj = new ExpendoObject();
            var respStr = "";
            try
            {
                if (_httpClient == null) _httpClient = new HttpClient();
                var resp = await _httpClient.GetAsync(url);
                respStr = resp.Content.ReadAsStringAsync().Result;
                dynamic respObj = JsonConvert.DeserializeObject<dynamic>(respStr);
                // var respObj = JObject
                // return Ok(respObj.openid);
                if (respObj.userid == null)
                    response = respObj.errmsg;
                else
                    response = respObj.userid;
            }
            catch (Exception e)
            {
                _logger.LogError(e, respStr);
                dynamic respObj = JsonConvert.DeserializeObject(respStr);
                response = respObj.errmsg;
            }

            return Ok(response);
        }



        [HttpGet]
        [Route("wxlogin/{jscode}")]
        public async Task<IActionResult> Login(string jscode)
        {
            var appid = _configuration.GetValue<string>("wx:appid");
            var appsec = _configuration.GetValue<string>("wx:appsec");
            var url = string.Format("https://api.weixin.qq.com/sns/jscode2session?appid={1}&secret={2}&js_code={0}&grant_type=authorization_code", jscode,appid,appsec);
            var responseCode = "";
            // dynamic respObj = new ExpendoObject();
            var respStr = "";
            try
            {
                if (_httpClient == null) _httpClient = new HttpClient();
                var resp = await _httpClient.GetAsync(url);
                respStr = resp.Content.ReadAsStringAsync().Result;
                dynamic respObj = JsonConvert.DeserializeObject(respStr);
                // var respObj = JObject
                // return Ok(respObj.openid);
                if (respObj.openid == null)
                    responseCode = respObj.errmsg;
                else
                    responseCode = respObj.openid;
            }
            catch (Exception e)
            {
                _logger.LogError(e, respStr);
                dynamic respObj = JsonConvert.DeserializeObject(respStr);
                responseCode = respObj.errmsg;
            }

            return Ok(responseCode);
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



        [HttpPost]
        [Route("saveFile")]
        public IActionResult SaveFile(SaveFileRequest request)
        {
            _logger.LogError(request.Content);
            var strDateTime = DateTime.Now.ToString("yyMMddhhmmssfff"); //取得时间字符串
            var path = Directory.GetCurrentDirectory() + @"\" + request.FileName;
            using (FileStream fs = new FileStream(path, FileMode.Create))
            {
                StreamWriter sw = new StreamWriter(fs);
                try
                {
                    sw.Write(request.Content);
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


        #region Rger
        [HttpGet]
        [Route("onviewuser/{actID}")]
        public IActionResult GetOnViewUser(string actID)
        {
            var act = ClientHub.ActivityList.Where(a => a.ActId == actID.Trim())?.FirstOrDefault();
            return Ok(act?.OnViewClientList);
        }

        #endregion Reger
    }
}