using DirectSp.Core.Entities;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace DirectSp.Core.Infrastructure
{
    public interface IDspKeyValue
    {
        Task<IEnumerable<DspKeyValueItem>> All(string keyNamePattern = null);

        Task SetValue(string keyName, string value, int timeToLife = 0, bool isOverwrite = true);

        Task<object> GetValue(string keyName);

    }
}
