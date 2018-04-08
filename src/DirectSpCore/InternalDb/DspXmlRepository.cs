using System.Collections.Generic;
using System.Xml.Linq;
using Microsoft.AspNetCore.DataProtection.Repositories;
using System.Linq;
using System.Threading.Tasks;

namespace DirectSp.Core.InternalDb
{
    public class DspXmlRepository : IXmlRepository
    {
        public string Name { get; private set; }
        public DspKeyValue SqlKeyValue { get; private set; }

        public DspXmlRepository(DspKeyValue sqlKeyValue, string name = "XmlRepo")
        {
            Name = name;
            SqlKeyValue = sqlKeyValue;
        }

        public IReadOnlyCollection<XElement> GetAllElements()
        {
            var res = SqlKeyValue.All($"{Name}/%").Result;
            var ret = res.Select(x => XElement.Parse(x.TextValue));
            return ret.ToArray();
        }

        public void StoreElement(XElement element, string friendlyName)
        {
            SqlKeyValue.ValueSet($"{Name}/{friendlyName}", element.ToString()).ConfigureAwait(false);
        }
    }
}