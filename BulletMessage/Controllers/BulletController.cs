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

        // [HttpPost("upload-file")]
        // public async Task<IActionResult> uploadFile(UploadFileData data)
        // {
        //     return Ok("Ok");
        // }

        [HttpGet]
        [Route("uploadfiles")]
        public IActionResult GetUploadFileList()
        {
            var webRootPath = _environment.WebRootPath;
            var filePath = "/Uploads/Images/";
            DirectoryInfo dir = new DirectoryInfo(webRootPath + filePath);
            var fileList = dir.GetFiles().Select(f => f.Name).ToList();
            return Ok(fileList);
        }

        [HttpPost]
        [Route("upload")]
        public async Task<IActionResult> uploadFile(string fileName)
        {
            _logger.LogDebug($"UploadFile");
            var uploadfile = Request.Form.Files[0];
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



                // //插入图片数据
                // var picture = new Picture
                // {
                //     MimeType = uploadfile.ContentType,
                //     AltAttribute = "",
                //     FilePath = filePath + saveName,
                //     CreatedDateTime = DateTime.Now
                // };
                // using (FileStream fs = System.IO.File.Create(webRootPath + filePath + saveName))
                // {
                //     uploadfile.CopyTo(fs);
                //     fs.Flush();
                // }
                // _pictureService.Insert(picture);
                // return Ok(new { isSuccess = true, returnMsg = "上传成功", imgId = picture.Id, imgUrl = picture.FilePath });
            }
            return Ok(new { isSucceed = true, resultMsg = "upload success" });
            // return Ok("Ok");
        }

        [HttpPost("upload2")]
        public async Task<IActionResult> Post(List<IFormFile> files)
        {
            long size = files.Sum(f => f.Length);

            // full path to file in temp location
            var filePath = Path.GetTempFileName();

            foreach (var formFile in files)
            {
                if (formFile.Length > 0)
                {
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await formFile.CopyToAsync(stream);
                    }
                }
            }

            // process uploaded files
            // Don't rely on or trust the FileName property without validation.

            return Ok(new { count = files.Count, size, filePath });
        }

        // /// <summary>
        // /// 上传图片
        // /// </summary>
        // /// <returns></returns>
        // [HttpPost]
        // public IActionResult UploadFileNew()
        // {
        //     _logger.LogDebug($"UploadFile");
        //     // ResultData result = new ResultData();
        //     string parameters = "";
        //     string operating = "图片上传";
        //     string path = "/tmp/";
        //     HttpPostedFile file = System.Web.HttpContext.Current.Request.Files["content"]; //对应小程序 name
        //     parameters = string.Format("postData:{0}", file.ToString());
        //     //_logger.LogInformation("file文件：" + file.ToString(), 0, "miapp", module, operating);
        //     //获取文件
        //     if (file != null)
        //     {
        //         Stream sr = file.InputStream;        //文件流
        //         Bitmap bitmap = (Bitmap)Bitmap.FromStream(sr);
        //         path += file.FileName;
        //         string currentpath = System.Web.HttpContext.Current.Server.MapPath("~");


        //         bitmap.Save(currentpath + path);
        //     }

        //     return Ok("upload complete");

        // }

    }
}