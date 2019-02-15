using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BulletMessage.Contract.Request;
using BulletMessage.Hubs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;
using BulletMessage.Contract.Model;
using System.Text.RegularExpressions;

namespace BulletMessage.Controllers
{
    // public class UploadFileData
    // {
    //     public IFormFile file { get; set; }
    // }

    [Route("api/[controller]")]
    [ApiController]
    public class BulletController : ControllerBase
    {
        private readonly IHubContext<BulletHub> _hubContext;
        private readonly IHubContext<ClientHub> _clientContext;
        private readonly ILogger<BulletController> _logger;
        private IHostingEnvironment _environment;

        public static List<Message> MessageHistory;

        private int _messageSize = 50;

        public BulletController(IHubContext<BulletHub> hubContext, IHubContext<ClientHub> clientContext, ILogger<BulletController> logger, IHostingEnvironment hostingEnvironment)
        {
            _environment = hostingEnvironment;
            _logger = logger;
            _hubContext = hubContext;
            _clientContext = clientContext;
            if (MessageHistory == null)
                MessageHistory = new List<Message>();
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
            //request.Id is avatorUrl
            _logger.LogDebug($"ReceiveMessage=>{request.Id}:{request.Message}");
            _hubContext.Clients.All.SendAsync("ReceiveMessage", request.Id, request.Message);
            _clientContext.Clients.All.SendAsync("ReceiveMessage", request.Id, request.Message, request.nickName);//send message to live chat room
            MessageHistory.Add(new Message()
            {
                NickName = request.nickName,
                EnglishName = request.englishName,
                Msg = request.Message,
                AvatarUrl = request.Id,
                TimeStamp = DateTime.Now
            });
            if (MessageHistory.Count() > _messageSize)
            {
                MessageHistory.RemoveAt(0);
            }
            return Ok(new { success = true });
        }


        // POST api/values
        [HttpPost]
        [Route("changebg")]
        public IActionResult ChangeBackground(BulletRequest request)
        {
            _logger.LogDebug($"changeBackground=>{request.Id}:{request.Message}");
            _hubContext.Clients.All.SendAsync("changeBackground", request.Message);
            return Ok(new { success = true });
        }

        [HttpGet]
        [Route("messageHistory")]
        public IActionResult GetMessageHistory()
        {
            if (MessageHistory.Count() > _messageSize)
            {
                MessageHistory = MessageHistory.OrderBy(m => m.TimeStamp).Take(30).ToList();
            }
            return Ok(MessageHistory);
        }

        [HttpGet]
        [Route("uploadfiles")]
        public IActionResult GetUploadFileList()
        {
            var webRootPath = _environment.WebRootPath;
            var filePath = "/Uploads/Images/";
            DirectoryInfo dir = new DirectoryInfo(webRootPath + filePath);

            // //判断后缀是否是图片
            // const string fileFilt = ".gif|.jpg|.php|.jsp|.jpeg|.png|......";
            // if (fileExtension == null)
            // {
            //     return Ok(new { isSucceed = false, resultMsg = "no extension" });
            // }
            // if (fileFilt.IndexOf(fileExtension.ToLower(), StringComparison.Ordinal) <= -1)
            // {
            //     return Ok(new { isSucceed = false, resultMsg = "no picture" });
            // }

            Regex regPic = new Regex(@".*\.(jpg|jpeg|png|gif)$");
            var files = dir.GetFiles()?.Where(f => regPic.IsMatch(f.Name.ToLower()))
            .OrderBy(f1 => f1.Name.Count())
            .ThenBy(f2 => f2.Name)
            .ToList();

            List<FileInfo> list = new List<FileInfo>(files);
            // list.Sort(new Comparison<FileInfo>(delegate (FileInfo a, FileInfo b)
            // {
            //     return b.CreationTime.CompareTo(a.CreationTime);

            // }));

            var fileList = list.Select(f => f.Name).ToList();
            return Ok(fileList);
        }

        [HttpPost]
        [Route("deletefiles")]
        public IActionResult DeleteUploadFile(DeleteFileRequest request)
        {
            var webRootPath = _environment.WebRootPath;
            var filePath = "/Uploads/Images/";
            DirectoryInfo dir = new DirectoryInfo(webRootPath + filePath);

            var targetFilePath = webRootPath + filePath + request.FileName;

            try
            {
                System.IO.File.Delete(targetFilePath);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                throw e;
            }
            return Ok("file delete" + targetFilePath);
        }

        [HttpPost]
        [Route("upload")]
        public async Task<IActionResult> uploadFile()
        {
            var uploadfile = Request.Form.Files[0];
            var nickName = Request.Form["nickName"].ToString();
            var englishName = Request.Form["englishName"].ToString();
            var openId = Request.Form["openid"].ToString();
            var avatarUrl = Request.Form["avatarUrl"].ToString();
            _logger.LogDebug($"UploadFile=>{nickName}=>{avatarUrl}");

            var now = DateTime.Now;
            var webRootPath = _environment.WebRootPath;
            // var webRootPath = Server.MapPath("~/");
            // var filePath = string.Format("/Uploads/Images/{0}/{1}/{2}/", now.ToString("yyyy"), now.ToString("yyyyMM"), now.ToString("yyyyMMdd"));
            var filePath = "/Uploads/Images/";

            if (!Directory.Exists(webRootPath + filePath))
            {
                Directory.CreateDirectory(webRootPath + filePath);
            }

            if (uploadfile != null)
            {
                //文件后缀
                var fileExtension = Path.GetExtension(uploadfile.FileName);

                //判断后缀是否是图片
                const string fileFilt = ".gif|.jpg|.php|.jsp|.jpeg|.png|......";
                if (fileExtension == null)
                {
                    return Ok(new { isSucceed = false, resultMsg = "no extension" });
                }
                if (fileFilt.IndexOf(fileExtension.ToLower(), StringComparison.Ordinal) <= -1)
                {
                    return Ok(new { isSucceed = false, resultMsg = "no picture" });
                }

                //判断文件大小    
                long length = uploadfile.Length;
                if (length > 1024 * 1024 * 10) //2M
                {
                    return Ok(new { isSucceed = false, resultMsg = "file exceed 5M" });
                }

                var strDateTime = DateTime.Now.ToString("yyMMddhhmmssfff"); //取得时间字符串
                var strRan = Convert.ToString(new Random().Next(100, 999)); //生成三位随机数
                var saveName = strDateTime + strRan + fileExtension;
                var avatarUrlShort = avatarUrl.Replace("https://wx.qlogo.cn/mmopen/vi_32/", "");
                avatarUrlShort = avatarUrlShort.Replace("\\132", "");
                avatarUrlShort = avatarUrlShort.Replace("/132", "");
                _logger.LogInformation("short cut=>" + avatarUrlShort);
                var fileName = englishName + "-_-" + strDateTime + "-_-" + avatarUrlShort;
                // _logger.LogInformation(fileName);
                var savePath = webRootPath + filePath + fileName + fileExtension;//".png";
                using (FileStream stream = new FileStream(savePath, FileMode.Create))
                {
                    await uploadfile.CopyToAsync(stream);
                }
                var prefix = "img=>";
                await _hubContext.Clients.All.SendAsync("ReceiveMessage", avatarUrl, prefix + fileName + fileExtension);

                var message = "img=>" + fileName + fileExtension;
                await _clientContext.Clients.All.SendAsync("ReceiveMessage", avatarUrl, message, englishName);//send message to live chat room
                MessageHistory.Add(new Message()
                {
                    NickName = englishName,
                    EnglishName = englishName,
                    Msg = message,
                    AvatarUrl = avatarUrl,
                    TimeStamp = DateTime.Now
                });
                if (MessageHistory.Count() > _messageSize)
                {
                    MessageHistory.RemoveAt(0);
                }
            }

            return Ok(new { isSucceed = true, resultMsg = "upload success" });
            // return Ok("Ok");
        }


        [HttpPost]
        [Route("uploadbg")]
        public async Task<IActionResult> uploadBgFile()
        {
            try
            {
                var uploadfile = Request.Form.Files[0];
                var nickName = Request.Form["nickName"].ToString();
                var englishName = Request.Form["englishName"].ToString();
                var openId = Request.Form["openid"].ToString();
                var avatarUrl = Request.Form["avatarUrl"].ToString();
                _logger.LogDebug($"UploadBgFile=>{nickName}=>{avatarUrl}");

                var now = DateTime.Now;
                var webRootPath = _environment.WebRootPath;
                var filePath = webRootPath + "/static/img/background/bg1.png";

                // if (!Directory.Exists(webRootPath + filePath))
                // {
                //     Directory.CreateDirectory(webRootPath + filePath);
                // }

                if (uploadfile != null)
                {
                    //delete the file first
                    System.IO.File.Delete(filePath);
                    //文件后缀
                    var fileExtension = Path.GetExtension(uploadfile.FileName);

                    //判断后缀是否是图片
                    const string fileFilt = ".gif|.jpg|.php|.jsp|.jpeg|.png|......";
                    if (fileExtension == null)
                    {
                        return Ok(new { isSucceed = false, resultMsg = "no extension" });
                    }
                    if (fileFilt.IndexOf(fileExtension.ToLower(), StringComparison.Ordinal) <= -1)
                    {
                        return Ok(new { isSucceed = false, resultMsg = "no picture" });
                    }

                    //判断文件大小    
                    long length = uploadfile.Length;
                    if (length > 1024 * 1024 * 10) //2M
                    {
                        return Ok(new { isSucceed = false, resultMsg = "file exceed 5M" });
                    }
                    using (FileStream stream = new FileStream(filePath, FileMode.Create))
                    {
                        await uploadfile.CopyToAsync(stream);
                    }
                }
                // return Ok("Ok");
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return Ok(new { isSucceed = true, resultMsg = "upload failure" });
            }

            return Ok(new { isSucceed = true, resultMsg = "upload success" });
        }


    }
}