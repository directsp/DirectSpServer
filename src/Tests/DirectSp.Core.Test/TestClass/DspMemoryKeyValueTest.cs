using DirectSp.Core.Entities;
using DirectSp.Core.Exceptions;
using DirectSp.Core.InternalDb;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DirectSp.Core.Test.TestClass
{
    [TestClass]
    public class DspMemoryKeyValueTest
    {
        [TestMethod]
        public async Task ExpirationTest()
        {
            var dspKeyValue = new DspMemoryKeyValue();
            int timetoLife = 1;
            await dspKeyValue.SetValue("TestKey01", "nothing else matter", timetoLife, false);
            await dspKeyValue.SetValue("TestKey02", "nothing else matter", timetoLife + 2, false);
            Thread.Sleep(TimeSpan.FromSeconds(1));
            try
            {
                var testValue01 = await dspKeyValue.GetValue("TestKey01");
            }
            catch (SpAccessDeniedOrObjectNotExistsException) { }

            var testValue02 = (DspKeyValueItem)await dspKeyValue.GetValue("TestKey02");
            Assert.IsTrue(testValue02.KeyName == "TestKey02");
        }

        [TestMethod]
        public async Task GetAllKeyValuesTest()
        {
            var dspKeyValue = new DspMemoryKeyValue();
            int timetoLife = 20;

            await dspKeyValue.SetValue("TestKey01", "nothing else matter", 1, false);
            await dspKeyValue.SetValue("TestKey02", "nothing else matter", timetoLife, false);
            await dspKeyValue.SetValue("TestKey03", "nothing else matter", timetoLife, false);
            await dspKeyValue.SetValue("keyTest01", "nothing else matter", 1, false);
            await dspKeyValue.SetValue("keyTest02", "nothing else matter", timetoLife, false);
            await dspKeyValue.SetValue("keyTest03", "nothing else matter", timetoLife, false);
            Thread.Sleep(TimeSpan.FromSeconds(1));

            var all = await dspKeyValue.All("");
            Assert.IsTrue(all.Count == 4);

            // All KeyValues that their keys started with Test keyword
            var allTestKeys = await dspKeyValue.All("Test");
            Assert.IsTrue(allTestKeys.Count == 2);

            // All KeyValues that their keys started with Key keyword
            var allKeyTests = await dspKeyValue.All("key");
            Assert.IsTrue(allKeyTests.Count == 2);

        }

        [TestMethod]
        public async Task SetValueTest()
        {
            var dspKeyValue = new DspMemoryKeyValue();
            int timetoLife = 20;

            await dspKeyValue.SetValue("TestKey01", "nothing else matter", timetoLife, false);
            await dspKeyValue.SetValue("TestKey02", "nothing else matter", timetoLife, false);

            // Check isOverwrite option
            try
            {
                await dspKeyValue.SetValue("TestKey01", "nothing else matter", timetoLife, false);
            }
            catch (SpObjectAlreadyExists) { }

            await dspKeyValue.SetValue("TestKey01", "updated", timetoLife, true);
            var value = (DspKeyValueItem)await dspKeyValue.GetValue("TestKey01");
            Assert.IsTrue(value.TextValue == "updated");

        }

        [TestMethod]
        public async Task DeleteTest()
        {
            var dspKeyValue = new DspMemoryKeyValue();
            int timetoLife = 20;

            await dspKeyValue.SetValue("TestKey01", "nothing else matter", timetoLife, false);
            await dspKeyValue.SetValue("TestKey02", "nothing else matter", timetoLife, false);

            try
            {
                await dspKeyValue.Delete("NotExistKey");
            }
            catch (SpAccessDeniedOrObjectNotExistsException) { }

            await dspKeyValue.Delete("TestKey01");
            try
            {
                var value = (DspKeyValueItem)await dspKeyValue.GetValue("TestKey01");
            }
            catch (SpAccessDeniedOrObjectNotExistsException) { }
        }

    }
}
