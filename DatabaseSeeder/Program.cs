using WebMatrix.WebData;

namespace DatabaseSeeder
{
    class Program
    {
        static void Main(string[] args)
        {
            //var maintenance = new MaintenanceImporter(@"c:\users\bendike\desktop\test2.csv");
	        //maintenance.Execute();

            var comps = new ComponentImporter(@"c:\users\bendike\desktop\components.csv");
            comps.Execute();
        }
    }
}
