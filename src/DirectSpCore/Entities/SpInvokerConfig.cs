using DirectSp.Core.Entities;
using DirectSp.Core.Infrastructure;

namespace DirectSp.Core
{
    public class SpInvokerConfig
    {
        public string ConnectionString { get; set; }
        public string Schema { get; set; }
        public SpInvokerOptions Options { get; set; }
        public IDspKeyValue KeyValue { get; set; }
        public IDbLayer DbLayer { get; set; }
        public JwtTokenSigner TokenSigner { get; set; }
    }
}
