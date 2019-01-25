using System.Collections.Generic;
using System.Xml.Linq;
using Microsoft.AspNetCore.DataProtection.Repositories;
using System.Linq;

namespace DirectSp.NetCore.Providers
{
    public class XmlRepositoryProvider : IXmlRepository
    {
        public string Name { get; private set; }
        public IKeyValueProvider SqlKeyValue { get; private set; }

        public XmlRepositoryProvider(IKeyValueProvider sqlKeyValue, string name = "XmlRepo")
        {
            Name = name;
            SqlKeyValue = sqlKeyValue;
        }

        public IReadOnlyCollection<XElement> GetAllElements()
        {
            var res = SqlKeyValue.All($"{Name}/").Result;
            var ret = res.Select(x => XElement.Parse(x.TextValue));
            return ret.ToArray();
        }

        public void StoreElement(XElement element, string friendlyName)
        {
            SqlKeyValue.SetValue($"{Name}/{friendlyName}", element.ToString()).ConfigureAwait(false);
        }
    }
}