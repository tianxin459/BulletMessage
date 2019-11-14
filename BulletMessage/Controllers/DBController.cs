using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BulletMessage.Contract.Model;
using BulletMessage.Contract.Request;
using LiteDB;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace BulletMessage.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DBController : ControllerBase
    {
        private readonly ILogger<ClientController> _logger;
        private string _pathDB = Directory.GetCurrentDirectory() + @"\Winner.db";
        public DBController(ILogger<ClientController> logger)
        {
            _logger = logger;
        }



        [HttpGet]
        [Route("ping")]
        public IActionResult Ping()
        {
            return Ok(DateTime.Now.ToString());
        }


        [HttpDelete]
        [Route("Winners")]
        public IActionResult clearAllWinners()
        {
            var dbName = "LuckyDrawWinners";
            List<WinnerUser> result = new List<WinnerUser>();
            using (var db = new LiteDatabase(_pathDB))
            {
                result = db.GetCollection<WinnerUser>(dbName).FindAll().ToList();
                foreach(var r in result)
                {
                    db.GetCollection<WinnerUser>(dbName).Delete(r.UserID);
                }
            }
            return Ok(result);
        }


        [HttpGet]
        [Route("Winners")]
        public IActionResult GetWinners()
        {
            _logger.LogInformation("get Winners");
            try
            {
                var dbName = "LuckyDrawWinners";
                List<WinnerUser> result = new List<WinnerUser>();
                using (var db = new LiteDatabase(_pathDB))
                {
                    result = db.GetCollection<WinnerUser>(dbName).FindAll().ToList();
                }
                return Ok(result);
            }
            catch(Exception e)
            {
                _logger.LogError(e.Message);
                throw e;
            }
        }


        [HttpPost]
        [Route("saveWinners")]
        public IActionResult SaveWinners(SaveWinnerRequest request)
        {
            var dbName = "LuckyDrawWinners";
            _logger.LogError(JsonConvert.SerializeObject(request));
            // Open database (or create if doesn't exist)
            using (var db = new LiteDatabase(_pathDB))
            {
                // Get a collection (or create, if doesn't exist)
                var col = db.GetCollection<WinnerUser>(dbName);
                foreach (var u in request.Winners)
                {
                    var result = col.Find(x => x.UserID == u.UserID).FirstOrDefault();
                    if (result == null)
                    {
                        col.Insert(u);
                    }
                    else
                    {
                        result = u;
                        col.Update(result);
                    }
                    col.EnsureIndex(x => x.UserID);

                }
                //var results = col.Find(x => x.);

                //// Insert new customer document (Id will be auto-incremented)
                //col.Insert(customer);

                //// Update a document inside a collection
                //customer.Name = "Joana Doe";

                //col.Update(customer);

                //// Index document using document Name property
                //col.EnsureIndex(x => x.Name);

                //// Use LINQ to query documents
                //var results = col.Find(x => x.Name.StartsWith("Jo"));

                //// Let's create an index in phone numbers (using expression). It's a multikey index
                //col.EnsureIndex(x => x.Phones, "$.Phones[*]");

                //// and now we can query phones
                //var r = col.FindOne(x => x.Phones.Contains("8888-5555"));
            }
            return Ok(new { Success = true });
        }
    }
}
