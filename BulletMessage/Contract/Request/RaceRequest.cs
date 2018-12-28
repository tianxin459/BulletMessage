using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace BulletMessage.Contract.Request
{
    public class RaceRequest
    {
        [DataMember(Name = "userId")]
        public string UserId { get; set; }
        [DataMember(Name = "avatorUrl")]
        public string AvatorUrl { get; set; }
        [DataMember(Name = "message")]
        public string Message { get; set; }
        [DataMember(Name = "englishName")]
        public string EnglishName { get; set; }
    }
}
