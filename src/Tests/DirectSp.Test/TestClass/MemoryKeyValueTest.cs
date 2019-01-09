using DirectSp.Exceptions;
using DirectSp.Providers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DirectSp.Core.Test.TestClass
{
    [TestClass]
    public class MemoryKeyValueTest
    {
        [TestMethod]
        public async Task ExpirationTest()
        {
            var keyValue = new MemoryKeyValueProvder();
            int timetoLife = 1;
            await keyValue.SetValue("TestKey01", "nothing else matter", timetoLife, false);
            await keyValue.SetValue("TestKey02", "nothing else matter", timetoLife + 2, false);
            Thread.Sleep(TimeSpan.FromSeconds(1));
            try
            {
                var testValue01 = await keyValue.GetValue("TestKey01");
            }
            catch (SpAccessDeniedOrObjectNotExistsException) { }

            var testValue02 = (KeyValueItem)await keyValue.GetValue("TestKey02");
            Assert.IsTrue(testValue02.KeyName == "TestKey02");
        }

        [TestMethod]
        public async Task GetAllKeyValuesTest()
        {
            var keyValue = new MemoryKeyValueProvder();
            int timetoLife = 20;

            await keyValue.SetValue("TestKey01", "nothing else matter", 1, false);
            await keyValue.SetValue("TestKey02", "nothing else matter", timetoLife, false);
            await keyValue.SetValue("TestKey03", "nothing else matter", timetoLife, false);
            await keyValue.SetValue("keyTest01", "nothing else matter", 1, false);
            await keyValue.SetValue("keyTest02", "nothing else matter", timetoLife, false);
            await keyValue.SetValue("keyTest03", "nothing else matter", timetoLife, false);
            await keyValue.SetValue("keyTest04", "nothing else matter", timetoLife, false);
            Thread.Sleep(TimeSpan.FromSeconds(1));

            var all = await keyValue.All("");
            Assert.IsTrue(all.Count == 5);

            // All KeyValues that their keys started with Test keyword
            var allTestKeys = await keyValue.All("Test");
            Assert.IsTrue(allTestKeys.Count == 2);

            // All KeyValues that their keys started with Key keyword
            var allKeyTests = await keyValue.All("key");
            Assert.IsTrue(allKeyTests.Count == 3);

        }

        [TestMethod]
        public async Task SetValueTest()
        {
            var keyValue = new MemoryKeyValueProvder();
            int timetoLife = 20;

            await keyValue.SetValue("TestKey01", "nothing else matter", timetoLife, false);
            await keyValue.SetValue("TestKey02", "nothing else matter", timetoLife, false);

            // Check isOverwrite option
            try
            {
                await keyValue.SetValue("TestKey01", "nothing else matter", timetoLife, false);
            }
            catch (SpObjectAlreadyExists) { }

            await keyValue.SetValue("TestKey01", "updated", timetoLife, true);
            var value = (KeyValueItem)await keyValue.GetValue("TestKey01");
            Assert.IsTrue(value.TextValue == "updated");
        }

        [TestMethod]
        public async Task DeleteTest()
        {
            var keyValue = new MemoryKeyValueProvder();
            int timetoLife = 20;

            await keyValue.SetValue("TestKey01", "nothing else matter", timetoLife, false);
            await keyValue.SetValue("TestKey02", "nothing else matter", timetoLife, false);

            try
            {
                await keyValue.Delete("NotExistKey");
            }
            catch (SpAccessDeniedOrObjectNotExistsException) { }

            await keyValue.Delete("TestKey01");
            try
            {
                var value = (KeyValueItem)await keyValue.GetValue("TestKey01");
            }
            catch (SpAccessDeniedOrObjectNotExistsException) { }
        }

    }
}
