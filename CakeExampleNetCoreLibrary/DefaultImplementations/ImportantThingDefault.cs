using CakeExampleNetCoreLibrary.Interfaces;

namespace CakeExampleNetCoreLibrary.DefaultImplementations
{
    public class ImportantThingDefault : IImportantThing
    {
        public void DoImportantThing()
        { }

        public object GetImportantThing()
        {
            return new object();
        }
    }
}