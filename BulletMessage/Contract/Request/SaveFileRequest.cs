using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace BulletMessage.Contract.Request
{
    public class SaveFileRequest
    {
        [DataMember(Name = "filename")]
        public string FileName { get; set; }
        [DataMember(Name = "content")]
        public string Content { get; set; }
    }
}
