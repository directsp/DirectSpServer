using DirectSp.Core.DI;
using DirectSp.Core.Test.DI;
using DirectSp.Core.Test.Mock;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
            string jwtToken = Data.JwtToken();

            var tokenSign = tokenSigner.Sign(jwtToken);

            Assert.IsTrue(tokenSigner.CheckSign(tokenSign));
        }
    }
}
