using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DirectSp.AuthServer.Entities
{
    public class UserCredential
    {
        public string UserId { get; set; }
        public string Password { get; set; }
        public string OneTimePassword { get; set; }
        public string IsOneTimePasswordExpired { get; set; }
    }
}
