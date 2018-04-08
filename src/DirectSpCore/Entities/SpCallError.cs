using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DirectSp.Core.Entities
{
    public class SpCallError
    {
        public string ErrorType { get; set; }
        public int ErrorNumber { get; set; }
        public string ErrorName { get; set; }
        public string ErrorMessage { get; set; }
        public string ErrorDescription { get; set; }
        public object ErrorProcName { get; set; }
        public object ErrorData { get; set; }
    }
}
