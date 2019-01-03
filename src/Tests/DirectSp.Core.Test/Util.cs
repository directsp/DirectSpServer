using System.Collections.Generic;
using System.ComponentModel;

namespace DirectSp.Core.Test
{
    static internal class Util
    {
        public static Dictionary<string, object> Dyn2Dict(object dynObj)
        {
            var dictionary = new Dictionary<string, object>();
            foreach (PropertyDescriptor propertyDescriptor in TypeDescriptor.GetProperties(dynObj))
            {
                object obj = propertyDescriptor.GetValue(dynObj);
                dictionary.Add(propertyDescriptor.Name, obj);
            }
            return dictionary;
        }
    }
}
