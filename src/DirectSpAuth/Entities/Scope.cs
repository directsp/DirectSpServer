using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DirectSpAuth.Entities
{
    public class Scope
    {
        public int ScopeId { get; set; }
        public int ApplicationId { get; set; }
        public string ScopeName { get; set; }
        public string Description { get; set; }
    }
}
