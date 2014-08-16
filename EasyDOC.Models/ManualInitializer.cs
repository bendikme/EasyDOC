using System.Data.Entity;

namespace EasyDOC.Model
{
    public class ManualInitializer : DropCreateDatabaseAlways<ManualContext>
    {
        protected override void Seed(ManualContext context)
        {
            /*var file = System.IO.File.ReadAllLines(@"c:\users\bendike\desktop\dok.csv", Encoding.UTF7);

            var first = true;

            var customers = new List<Customer>();

            foreach (var line in file)
            {
                if (first)
                {
                    first = false;
                    continue;
                }

                var columns = line.Split(';');

                if (!(columns.Length > 6 && columns[0].Length > 3 && columns[3].Length > 3 && columns[6].Length > 3))
                    continue;

                DateTime date;

                if (!DateTime.TryParse(columns[2], out date))
                    continue;

                string projectNumber;

                var id = columns[0];
                if (int.Parse(id[0].ToString(CultureInfo.InvariantCulture)) >= 8)
                {
                    projectNumber = "O" + columns[0].Substring(0, 2) + "-" + columns[0].Substring(2);
                }
                else
                {

                    projectNumber = "P" + columns[0].Substring(0, 4) + "-" + columns[0].Substring(4);
                }

                var project = new Project
                    {
                        ProjectNumber = projectNumber,
                        Created = DateTime.Parse(columns[2], CultureInfo.CurrentCulture),
                        Name = columns[6]
                    };

                context.Projects.Add(project);

                Customer customer;
                var asciiName = columns[3].Normalize().Trim();

                if ((customer = customers.SingleOrDefault(c => c.Name == asciiName)) == null)
                {
                    customer = new Customer
                    {
                        Name = asciiName
                    };

                    customers.Add(customer);
                }

				customer.Projects.Add(project);

            }

            foreach (var c in customers)
            {
                context.Customers.Add(c);
            }

            context.Categories.Add(new Category
            {
                Name = "No category"
            });

            context.Types.Add(new Type
            {
                Name = "No type"
            });

            context.Vendors.Add(new Vendor
            {
                Name = "No vendor"
            });

            context.SaveChanges();*/
        }
    }
}