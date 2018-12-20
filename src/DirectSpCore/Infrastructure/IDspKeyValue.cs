using DirectSp.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DirectSp.Core.Infrastructure
{
    public interface IDspKeyValue
    {
        Task<List<DspKeyValueItem>> All(string keyNamePattern = null);
        Task SetValue(string keyName, string value, int timeToLife = 0, bool isOverwrite = true);
        Task<object> GetValue(string keyName);
        Task<bool> Delete(string keyNamePattern);
    }
}
