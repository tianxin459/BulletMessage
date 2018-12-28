

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace BulletMessage.Contract.Request
{
    public class RaceResultRequest
    {
        [DataMember(Name = "value")]
        public string Value { get; set; }
    }
}
