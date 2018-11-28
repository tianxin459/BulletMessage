using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BulletMessage.Contract.Request
{
    public class BulletRequest
    {
        //id
        public string nickName { get; set; }
        public string Id { get; set; }
        public string Message { get; set; }
    }
}
