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
        private readonly ILogger<BulletController> _logger;
        private IHostingEnvironment _environment;

        public BulletController(IHubContext<BulletHub> hubContext, ILogger<BulletController> logger, IHostingEnvironment hostingEnvironment)
        {
            _environment = hostingEnvironment;
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
            _logger.LogDebug($"ReceiveMessage=>{request.Id}:{request.Message}");
            _hubContext.Clients.All.SendAsync("ReceiveMessage", request.Id, request.Message);
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



        [HttpPost]
        [Route("upload")]
        public async Task<IActionResult> uploadFile()
        {
            _logger.LogDebug($"UploadFile");
            var uploadfile = Request.Form.Files[0];
            var nickName = Request.Form["nickName"].ToString();
            var avatarUrl = Request.Form["avatarUrl"].ToString();

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
                if (length > 1024 * 1024 * 5) //2M
                {
                    return Ok(new { isSucceed = false, resultMsg = "file exceed 5M" });
                }

                var strDateTime = DateTime.Now.ToString("yyMMddhhmmssfff"); //取得时间字符串
                var strRan = Convert.ToString(new Random().Next(100, 999)); //生成三位随机数
                var saveName = strDateTime + strRan + fileExtension;
                var savePath = webRootPath + filePath + strDateTime + ".png";
                using (FileStream stream = new FileStream(savePath, FileMode.Create))
                {
                    await uploadfile.CopyToAsync(stream);
                }
                
            }
            return Ok(new { isSucceed = true, resultMsg = "upload success" });
            // return Ok("Ok");
        }
        
    }
}