using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace BulletMessage.Contract.Request
{
    public class ConfigureRequest
    {
        [DataMember(Name = "configurationJson")]
        public string ConfigurationJson { get; set; }
    }
}
