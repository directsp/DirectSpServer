using DirectSp.Core.Test.Mock;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DirectSp.Core.Test
{
    [TestClass]
    public class JwtTokenSignerTest
    {
        private JwtTokenSigner _tokenSigner;

        [TestInitialize]
        public void Init()
        {
            // Resolve SpInvoker internal dependencies
            _tokenSigner = new JwtTokenSigner(new Mock.CertificateProvider());
        }

        [TestMethod]
        public void TestSign()
        {
            #region
            //var o = new
            //{
            //    OrderNumber = 2,
            //    ReceiverLoyaltyAccountId = 11037931,
            //    ClubName = "Nike (باشگاه)",
            //    Amount = 1,
            //    PointTypeId = 2020,
            //    PointTypeName = "نقدی",
            //    PayeeLoyaltyAccountId = 1183,
            //    PayeeLoyaltyAccountName = "بهنام عیوض پور",
            //    exp = DateTime.Now.AddDays(1).ToUnixDate(),
            //    ReturnUrl = "http://www.google.com",
            //    CertificateThumb = "77 5a 92 92 f1 9f 14 fc 2b 9b 80 ab 06 2a 06 be 86 23 61 9c"
            //};

            //var tokenSign = _tokenSigner.Sign(JsonConvert.SerializeObject(o));
            #endregion

            var tokenSign = _tokenSigner.Sign(Data.JwtToken());
            Assert.IsTrue(_tokenSigner.CheckSign(tokenSign));
        }
    }
}
