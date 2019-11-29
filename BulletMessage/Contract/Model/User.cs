using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace BulletMessage.Contract.Model
{
    public class User
    {
        public string NickName { get; set; }
        public string EnglishName { get; set; }
        public string AvatarUrl { get; set; }
        public string Email { get; set; }
        public string Model { get; set; }
    }
    public class WinnerUser:User
    {
        [BsonId]
        [DataMember(Name = "userid")]
        public string UserID { get; set; }
        [DataMember(Name = "prize")]
        public string Prize { get; set; }
        [DataMember(Name = "department")]
        public string Department { get; set; }
        [DataMember(Name = "eventid")]
        public string EventId { get; set; }
        [DataMember(Name = "claimed")]
        public bool Claimed { get; set; } = false;
    }
}
