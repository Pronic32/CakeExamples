using CakeExampleNetCoreLibrary.DefaultImplementations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CakeExampleNetCoreTests
{
    [TestClass]
    public class ImportantThingDefaultTests
    {
        [TestMethod]
        public void GetImportantThingTest()
        {
            var thing = new ImportantThingDefault();
            
            var result = thing.GetImportantThing();

            Assert.IsNotNull(result);
        }
    }
}
