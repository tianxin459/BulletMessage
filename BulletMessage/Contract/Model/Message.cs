using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace BulletMessage.Contract.Model
{
    public class Message
    {
        [DataMember(Name = "openid")]
        public string OpenId { get; set; }
        [DataMember(Name = "nickName")]
        public string NickName { get; set; }
        [DataMember(Name = "englishName")]
        public string EnglishName { get; set; }
        [DataMember(Name = "message")]
        public string Msg { get; set; }
        [DataMember(Name = "avatarUrl")]
        public string AvatarUrl { get; set; }
        [DataMember(Name = "timeStamp")]
        public DateTime TimeStamp { get; set; }
    }
}