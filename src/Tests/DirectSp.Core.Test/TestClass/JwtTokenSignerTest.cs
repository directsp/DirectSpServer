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
                ReceiverLoyaltyAccountId = 11059317,
                ClubName = "Nike (باشگاه)",
                Amount = 25000,
                PointTypeId = 2766,
                PointTypeName = "نقدی",
                PayeeLoyaltyAccountId = 1183,
                PayeeLoyaltyAccountName = "بهنام عیوض پور",
                exp = DateTime.Now.AddDays(10).ToUnixDate(),
                ReturnUrl = "http://www.google.com",
                CertificateThumb = "93A152E2CD70A3782558A9EC8EBDC691BBB42F48"
            };

            var tokenSign = tokenSigner.Sign(JsonConvert.SerializeObject(o));
            Assert.IsTrue(tokenSigner.CheckSign(tokenSign));
        }
    }
}
