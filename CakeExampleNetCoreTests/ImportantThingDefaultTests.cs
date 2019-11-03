using CakeExampleNetCoreLibrary.DefaultImplementations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CakeExampleNetCoreTests
{
    [TestClass]
    public class ImportantThingDefaultTests
    {
        [TestMethod]
        public void GetImportantThingTest_ReturnsString()
        {
            var thing = new ImportantThingDefault();
            
            var result = thing.GetImportantThing(-100);

            Assert.AreEqual("magic string!", result);
        }

        [TestMethod]
        public void GetImportantThingTest_ReturnsDouble()
        {
            var thing = new ImportantThingDefault();
            
            var result = thing.GetImportantThing(10);

            Assert.AreEqual(3.5, result);
        }

        [TestMethod]
        public void GetImportantThingTest_ReturnsInt()
        {
            var thing = new ImportantThingDefault();
            
            var result = thing.GetImportantThing(1000);

            Assert.AreEqual(-1, result);
        }
    }
}
