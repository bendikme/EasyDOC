using System;
using System.Collections.Generic;
using System.Linq;
using EasyDOC.DAL.DataAccess;
using EasyDOC.Model;
using EasyDOC.Pdf.Interface;

namespace EasyDOC.Pdf.ConfirmationOrderParsers
{
    public class MurrElektronik : AbstractOrderConfirmationParser
    {
        public string PdfConverterRules { get; set; }

        public MurrElektronik(string rootDirectory, string filename) : base(rootDirectory, filename) {}

        public override OrderConfirmation ExtractOrderConfirmationData(IUnitOfWork uow)
        {
            var mainTable = ParsePdfColumns(PdfConverterRules);

            var currentLine = 0;
            var fetching = false;
            var fetchFrom = 0;
            var fetchAreas = new List<Tuple<int, int>>();

            var skippingHeader = true;

            // Find areas to include
            while (currentLine < mainTable.Count())
            {
                var parts = mainTable[currentLine];

                if (skippingHeader && parts[0] == "Pos.")
                {
                    skippingHeader = false;
                }

                int line;
                if (fetching && (int.TryParse(parts[0], out line) || parts[0].StartsWith("Murrelektronik as")))
                {
                    fetching = false;
                    fetchAreas.Add(new Tuple<int, int>(fetchFrom, currentLine - 1));
                }

                if (!skippingHeader && !fetching && int.TryParse(parts[0], out line))
                {
                    fetching = true;
                    fetchFrom = currentLine;
                }

                ++currentLine;
            }

            var orderItems = new List<OrderConfirmationItem>();

            foreach (var area in fetchAreas)
            {
                var article = "";
                var description = "";

                for (int i = 0; i <= area.Item2 - area.Item1; ++i)
                {
                    var row = mainTable[i + area.Item1];

                    if (i == 0)
                    {
                        article = row[1];
                    }
                    else if (row[1].Contains("STK"))
                    {
                        var part = row[1].Substring(0, row[1].IndexOf("STK", StringComparison.Ordinal)).Trim();
                        int count;

                        if (int.TryParse(part, out count))
                        {
                            /*orderItems.Add(new OrderConfirmationItem
                            {
                                Article = article,
                                Description = description,
                                Count = count
                            });*/
                        }

                        break;
                    }
                    else if (i > 2)
                    {
                        description += row[1] + "\n";
                    }
                }
            }

            return null;
        }
    }
}