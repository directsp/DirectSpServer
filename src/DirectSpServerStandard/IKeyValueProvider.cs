using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DirectSp
{
    public class KeyValueItem
    {
        public string KeyName { get; set; }
        public string TextValue { get; set; }
        public DateTime? ModifiedTime { get; set; }
    }

    public interface IKeyValueProvider
    {
        Task<List<KeyValueItem>> All(string keyNamePattern = null);
        Task SetValue(string keyName, string value, int timeToLife = 0, bool isOverwrite = true);
        Task<object> GetValue(string keyName);
        Task<bool> Delete(string keyNamePattern);
    }
}
