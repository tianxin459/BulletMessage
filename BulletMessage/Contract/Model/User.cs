﻿using System;
using System.Collections.Generic;
using System.Linq;
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
}
