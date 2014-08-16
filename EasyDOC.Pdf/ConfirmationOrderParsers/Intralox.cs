using System.Collections.Generic;
using System.Linq;
using EasyDOC.DAL.DataAccess;
using EasyDOC.Model;
using EasyDOC.Pdf.Interface;

namespace EasyDOC.Pdf.ConfirmationOrderParsers
{
    public class Intralox : AbstractOrderConfirmationParser
    {
        public string PdfConverterRules { get; set; }
        public Intralox(string rootDirectory, string filename) : base(rootDirectory, filename) { }


        public override OrderConfirmation ExtractOrderConfirmationData(IUnitOfWork uow)
        {
            /*var list = new List<System.Tuple<int, string, string, string>>();
            var table = ParsePdfColumns(PdfConverterRules, filename);

            var currentLine = 0;
            var fetching = false;
            var fetchFrom = 0;
            var fetchAreas = new List<System.Tuple<int, int>>();

            // Find areas to include
            while (currentLine < table.Count())
            {
                var parts = table[currentLine];

                if (!fetching && parts[0] == "Nr.")
                {
                    fetching = true;
                    fetchFrom = currentLine + 1;
                }
                else if (fetching && parts[0].Trim() == "" && parts[1] == "" && parts[2] == "" && parts[3].Trim() == "")
                {
                    fetching = false;
                    fetchAreas.Add(new System.Tuple<int, int>(fetchFrom, currentLine));
                }

                ++currentLine;
            }

            foreach (var area in fetchAreas)
            {
                var number = 0;

                for (int i = area.Item1; i < area.Item2; ++i)
                {
                    var parts = table[i];

                    int parsedNumber;
                    if (int.TryParse(parts[0], out parsedNumber))
                    {
                        number = parsedNumber;
                    }

                    list.Add(new System.Tuple<int, string, string, string>(number, parts[1], parts[2], parts[3]));
                }
            }

            return list
                .GroupBy(g => g.Item1)
                .Select(comp => new OrderConfirmationItem
                {
                    Count = int.Parse(comp.First().Item2),
                    Article = comp.First().Item3,
                    Description = comp.Select(g => g.Item4).Aggregate("", (current, g) => current + g + "\n")
                });*/

            return null;
        }
    }
}