//using DirectSp.Core.Test.Mock;
//using Microsoft.VisualStudio.TestTools.UnitTesting;

//namespace DirectSp.Core.Test.TestClass
//{
//    [TestClass]
//    public class JwtTokenSignerTest
//    {
//        private JwtTokenSigner _tokenSigner;

//        [TestInitialize]
//        public void Init()
//        {
//            // Resolve SpInvoker internal dependencies
//            _tokenSigner = new JwtTokenSigner(new Mock.CertificateProvider());
//        }

//        [TestMethod]
//        public void TestSign()
//        {
//            var tokenSign = _tokenSigner.Sign(Data.JwtToken);
//            Assert.IsTrue(_tokenSigner.CheckSign(tokenSign));
//        }
//    }
//}
