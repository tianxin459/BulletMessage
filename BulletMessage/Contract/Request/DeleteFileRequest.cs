using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;


namespace BulletMessage.Contract.Request
{
    public class DeleteFileRequest
    {

        [DataMember(Name = "fileName")]
        public string FileName { get; set; }
    }
}