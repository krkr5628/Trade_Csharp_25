using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using WindowsFormsApp1;

namespace Trade_Csharp_25.Tests
{
    [TestClass]
    public class FormTests
    {
        [TestMethod]
        public void TradeAuto_HasAuthenticationKey()
        {
            Assert.IsFalse(string.IsNullOrWhiteSpace(Trade_Auto.Authentication));
        }

        [TestMethod]
        public void SettingForm_HasTradeAutoConstructor()
        {
            var ctor = typeof(Setting).GetConstructor(new[] { typeof(Trade_Auto) });
            Assert.IsNotNull(ctor);
        }

        [TestMethod]
        public void TransactionForm_HasParameterlessConstructor()
        {
            var ctor = typeof(Transaction).GetConstructor(Type.EmptyTypes);
            Assert.IsNotNull(ctor);
        }

        [TestMethod]
        public void LogForm_HasParameterlessConstructor()
        {
            var ctor = typeof(Log).GetConstructor(Type.EmptyTypes);
            Assert.IsNotNull(ctor);
        }

        [TestMethod]
        public void UpdateForm_HasTradeAutoConstructor()
        {
            var ctor = typeof(Update).GetConstructor(new[] { typeof(Trade_Auto) });
            Assert.IsNotNull(ctor);
        }
    }
}
