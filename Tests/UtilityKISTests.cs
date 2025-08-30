using Microsoft.VisualStudio.TestTools.UnitTesting;
using WindowsFormsApp1;

namespace Trade_Csharp_25.Tests
{
    [TestClass]
    public class UtilityKISTests
    {
        [TestMethod]
        public void Constructor_InitializesFieldsFromUtility()
        {
            utility.KIS_Account = "12345678-01";
            utility.KIS_appkey = "app";
            utility.KIS_appsecret = "secret";

            var kis = new utility_KIS();

            Assert.AreEqual("12345678", kis.cano);
            Assert.AreEqual("01", kis.acntPrdtCd);
            Assert.AreEqual("app", kis.appKey);
            Assert.AreEqual("secret", kis.secretkey);
        }
    }
}
