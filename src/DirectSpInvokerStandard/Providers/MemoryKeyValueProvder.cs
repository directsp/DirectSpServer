using DirectSp.Exceptions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace DirectSp.Providers
{
    public class MemoryKeyValueProvder : IKeyValueProvider
    {
        class MemoryKeyValueItem
        {
            public string Value { get; set; }
            public DateTime ModifiedTime { get; set; }
            public DateTime ExpirationTime { get; set; }
        }

        private readonly ConcurrentDictionary<string, MemoryKeyValueItem> _keyValueItems = new ConcurrentDictionary<string, MemoryKeyValueItem>();
        private readonly Timer _timer = new Timer(30 * 60 * 1000);// 30 minutes

        public MemoryKeyValueProvder()
        {
            _timer.Elapsed += Timer_Elapsed;
            _timer.Start();
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            // Cleanup expired KeyValues
            Cleanup();
        }

        public Task<List<KeyValueItem>> All(string keyNamePattern = null)
        {
            var all = _keyValueItems.Where(item => item.Value.ExpirationTime > DateTime.Now
            && (item.Key.StartsWith(keyNamePattern) || string.IsNullOrEmpty(keyNamePattern))).Select(item =>
                  {
                      return new KeyValueItem
                      {
                          KeyName = item.Key,
                          ModifiedTime = item.Value.ModifiedTime,
                          TextValue = item.Value.Value
                      };
                  });
            return Task.FromResult<List<KeyValueItem>>(all.ToList());
        }

        public Task<object> GetValue(string keyName)
        {
            MemoryKeyValueItem item;
            if (!_keyValueItems.Keys.Contains(keyName) || (item = _keyValueItems[keyName]).ExpirationTime < DateTime.Now)
                throw new SpAccessDeniedOrObjectNotExistsException();

            return Task.FromResult<object>(new KeyValueItem
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

            var dspMemoryKeyValueItem = new MemoryKeyValueItem
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
            foreach (var key in expiredItems)
                _keyValueItems.TryRemove(key, out _);
        }

        public async Task<bool> Delete(string keyNamePattern)
        {
            var matchItems = await All(keyNamePattern);

            if (matchItems.Count == 0)
                return false;

            bool result = false;
            foreach (var item in matchItems)
                result = _keyValueItems.TryRemove(item.KeyName, out _);
            return result;
        }
    }
}
