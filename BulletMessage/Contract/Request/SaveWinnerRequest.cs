using BulletMessage.Contract.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace BulletMessage.Contract.Request
{
    public class SaveWinnerRequest
    {
        [DataMember(Name = "winners")]
        public List<WinnerUser> Winners = new List<WinnerUser>();
    }
}
