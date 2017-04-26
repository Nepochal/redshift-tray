using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace redshift_tray.Test
{
    [TestClass]
    public class CommonTests
    {
        [TestMethod]
        public void LocationCanBeParsed()
        {
            AutoLocation autoLocation = Common.ParseLocation("50.8476", "4.3428");
            Assert.AreEqual(50.8476M, autoLocation.Latitude);
            Assert.AreEqual(4.3428M, autoLocation.Longitude);
        }
    }
}
