using CakeExampleNetCoreLibrary.Interfaces;

namespace CakeExampleNetCoreLibrary.DefaultImplementations
{
    public class ImportantThingDefault : IImportantThing
    {
        public void DoImportantThing()
        { }

        public object GetImportantThing(int magicNumber)
        {
            if (magicNumber < 0)
            {
                return "magic string!";
            }

            if (magicNumber < 100)
            {
                return (double)3.5;
            }

            return -1;
        }
    }
}