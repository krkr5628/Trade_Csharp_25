using Microsoft.VisualStudio.TestTools.UnitTesting;
using WindowsFormsApp1;

namespace Trade_Csharp_25.Tests
{
    [TestClass]
    public class UtilityTests
    {
        [TestMethod]
        public void LoadCheck_DefaultsToFalse()
        {
            Assert.IsFalse(utility.load_check);
        }

        [TestMethod]
        public void SystemRoute_EndsWithSettingTxt()
        {
            StringAssert.EndsWith(utility.system_route, "setting.txt");
        }
    }
}
