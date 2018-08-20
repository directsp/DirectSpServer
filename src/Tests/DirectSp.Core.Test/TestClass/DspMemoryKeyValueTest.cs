using DirectSp.Core.Infrastructure;
using DirectSp.Core.InternalDb;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DirectSp.Core.Test.TestClass
{
    [TestClass]
    class DspMemoryKeyValueTest
    {
        private IDspKeyValue _dspKeyValue;

        [TestInitialize]
        void Init()
        {
            // Resolve _dspKeyValue
            _dspKeyValue = new DspMemoryKeyValue();
        }



    }
}
