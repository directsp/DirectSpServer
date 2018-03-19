using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace DirectSpAuth.Entities
{
    public class Application
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string ApplicationName { get; set; }
        public string LogoutRedirectUri { get; set; }
        public string[] RedirectUris { get; set; }
        public Scope[] Scopes { get; set; }
    }
}