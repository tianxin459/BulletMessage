using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace BulletMessage.Contract.Request
{
    public class ClientRequest
    {
        [DataMember(Name = "nickName")]
        public string NickName { get; set; }
        [DataMember(Name = "email")]
        public string Email { get; set; }
        [DataMember(Name = "englishName")]
        public string EnglishName { get; set; }
        [DataMember(Name = "avatarUrl")]
        public string AvatarUrl { get; set; }
        [DataMember(Name = "model")]
        public string Model { get; set; }
    }
}
