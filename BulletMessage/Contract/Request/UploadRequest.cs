using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace BulletMessage.Contract.Request
{
    public class UploadRequest
    {
        [DataMember(Name = "nickName")]
        public string UserID { get; set; }

        [DataMember(Name = "avatarUrl")]
        public string AvatarUrl { get; set; }
    }
}
