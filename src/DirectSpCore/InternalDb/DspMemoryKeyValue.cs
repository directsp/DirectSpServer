using DirectSp.Core.Entities;
using DirectSp.Core.Exceptions;
using DirectSp.Core.Infrastructure;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectSp.Core.InternalDb
{
    public class DspMemoryKeyValue : IDspKeyValue
    {
        class DspMemoryKeyValueItem
        {
            public string Value { get; set; }
            public DateTime ModifiedTime { get; set; }
            public DateTime ExpirationTime { get; set; }
        }

        private ConcurrentDictionary<string, DspMemoryKeyValueItem> _keyValueItems = new ConcurrentDictionary<string, DspMemoryKeyValueItem>();


        public Task<IEnumerable<DspKeyValueItem>> All(string keyNamePattern = null)
        {
            var all = _keyValueItems.Keys.Where(key => key.StartsWith(keyNamePattern)).Select(key =>
            {
                var item = _keyValueItems[key];
                return new DspKeyValueItem
                {
                    KeyName = key,
                    ModifiedTime = item.ModifiedTime,
                    TextValue = item.Value
                };
            });
            return Task.FromResult(all);
        }

        public Task<object> GetValue(string keyName)
        {
            var item = _keyValueItems[keyName];
            return Task.FromResult<object>(new DspKeyValueItem
            {
                KeyName = keyName,
                ModifiedTime = item.ModifiedTime,
                TextValue = item.Value
            });
        }

        public Task SetValue(string keyName, string value, int timeToLife = 0, bool isOverwrite = true)
        {
            if (_keyValueItems.Keys.Contains(keyName) && !isOverwrite)
                throw new SpObjectAlreadyExists();

            var dspMemoryKeyValueItem = new DspMemoryKeyValueItem
            {
                ExpirationTime = DateTime.Now.AddMinutes(timeToLife),
                Value = value
            };

            _keyValueItems.AddOrUpdate(keyName, dspMemoryKeyValueItem, (key, itemValue) =>
            {
                dspMemoryKeyValueItem.ModifiedTime = DateTime.Now;
                return dspMemoryKeyValueItem;
            });


            return Task.FromResult<object>(null);
        }

    }
}
