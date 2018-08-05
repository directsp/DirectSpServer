using DirectSp.Core.DI;
using DirectSp.Core.Test.DI;
using DirectSp.Core.Test.Mock;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Newtonsoft.Json;
using DirectSp.Core.Helpers;

namespace DirectSp.Core.Test
{
    [TestClass]
    public class JwtTokenSignerTest
    {
        [TestInitialize]
        public void Init()
        {
            Resolver.Instance.SetModule(new TestModule());
        }

        [TestMethod]
        public void TestSign()
        {
            var tokenSigner = Resolver.Instance.Resolve<JwtTokenSigner>();
            //string jwtToken = Data.JwtToken();
            //var tokenSign = tokenSigner.Sign(jwtToken);

            var o = new
            {
                OrderNumber = 2,
                ReceiverLoyaltyAccountId = 11037931,
                ClubName = "Nike (باشگاه)",
                Amount = 1,
                PointTypeId = 2020,
                PointTypeName = "نقدی",
                PayeeLoyaltyAccountId = 1183,
                PayeeLoyaltyAccountName = "بهنام عیوض پور",
                exp = DateTime.Now.AddDays(1).ToUnixDate(),
                ReturnUrl = "http://www.google.com",
                CertificateThumb = "775a9292f19f14fc2b9b80ab062a06be8623619c"
            };

            var tokenSign = tokenSigner.Sign(JsonConvert.SerializeObject(o));
            Assert.IsTrue(tokenSigner.CheckSign(tokenSign));
        }
    }
}
