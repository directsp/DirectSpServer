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

        public Task<List<DspKeyValueItem>> All(string keyNamePattern = null)
        {
            var all = _keyValueItems.Where(item => item.Value.ExpirationTime > DateTime.Now
            && (item.Key.StartsWith(keyNamePattern) || string.IsNullOrEmpty(keyNamePattern))).Select(item =>
                  {
                      return new DspKeyValueItem
                      {
                          KeyName = item.Key,
                          ModifiedTime = item.Value.ModifiedTime,
                          TextValue = item.Value.Value
                      };
                  });
            return Task.FromResult<List<DspKeyValueItem>>(all.ToList());
        }

        public Task<object> GetValue(string keyName)
        {
            DspMemoryKeyValueItem item;
            if (!_keyValueItems.Keys.Contains(keyName) || (item = _keyValueItems[keyName]).ExpirationTime < DateTime.Now)
                throw new SpAccessDeniedOrObjectNotExistsException();

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

            // Cleanup expired KeyValues
            Cleanup();

            var dspMemoryKeyValueItem = new DspMemoryKeyValueItem
            {
                ExpirationTime = DateTime.Now.AddSeconds(timeToLife),
                Value = value
            };

            _keyValueItems.AddOrUpdate(keyName, dspMemoryKeyValueItem, (key, itemValue) =>
            {
                dspMemoryKeyValueItem.ModifiedTime = DateTime.Now;
                return dspMemoryKeyValueItem;
            });

            return Task.FromResult<object>(null);
        }

        private void Cleanup()
        {
            var expiredItems = _keyValueItems.Where(item => item.Value.ExpirationTime < DateTime.Now).Select(item => item.Key);
            DspMemoryKeyValueItem value;
            foreach (var key in expiredItems)
                _keyValueItems.Remove(key, out value);
        }

        public async Task<bool> Delete(string keyNamePattern)
        {
            var matchItems = await All(keyNamePattern);

            if (matchItems.Count == 0)
                return false;

            bool result = false;
            DspMemoryKeyValueItem value;
            foreach (var item in matchItems)
                result = _keyValueItems.TryRemove(item.KeyName, out value);
            return result;
        }
    }
}
