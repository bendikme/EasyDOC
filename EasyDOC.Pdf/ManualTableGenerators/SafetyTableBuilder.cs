using System;
using System.Collections.Generic;
using System.Linq;
using EasyDOC.Model;
using EasyDOC.Pdf.Interface;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.Ajax.Utilities;

namespace EasyDOC.Pdf.ManualTableGenerators
{
    class SafetyTableBuilder : ITableBuilder
    {
        public Documentation Documentation { get; set; }
        public string RootDirectory { get; set; }
        public string Titles { get; set; }
        public string Widths { get; set; }
        public string Role { get; set; }

        public bool HasMultipleTables
        {
            get { return false; }
        }

        public IReadOnlyDictionary<string, string> Parameters { get; set; }

        public IEnumerable<DocumentTable> GetTables()
        {
            var table = new PdfPTable(3)
            {
                WidthPercentage = 100
            };

            table.SetWidths(Widths.Split(',').Select(int.Parse).ToArray());

            table.DefaultCell.BackgroundColor = new GrayColor(0.9f);
            table.DefaultCell.SetLeading(15, 0);
            table.DefaultCell.Padding = 5.0f;

            var titles = Titles.Split(',');

            table.AddCell(new Phrase(titles[0], PdfFonts.Bold));
            table.AddCell(new Phrase(titles[1], PdfFonts.Bold));
            table.AddCell(new Phrase(titles[2], PdfFonts.Bold));

            table.HeaderRows = 1;
            table.DefaultCell.BackgroundColor = BaseColor.WHITE;

            foreach (var rel in GetAllSafety())
            {
                table.AddCell(new Phrase(rel.Safety.Description, PdfFonts.Normal));
                table.AddCell(new Phrase(rel.Location, PdfFonts.Normal));

                if (rel.Safety.Image != null)
                {
                    var fullName = string.Format(@"{0}\{1}", RootDirectory, rel.Safety.Image.Path);

                    try
                    {
                        var image = Image.GetInstance(fullName);
                        image.ScaleToFit(Math.Min(150, image.Width), Math.Min(150, image.Height));

                        var cell = new PdfPCell(image)
                        {
                            HorizontalAlignment = Element.ALIGN_CENTER,
                            VerticalAlignment = Element.ALIGN_MIDDLE,
                            Padding = 5.0f
                        };

                        table.AddCell(cell);
                    }
                    catch
                    {
                        table.AddCell(new Phrase("BILDET " + fullName + " IKKE FUNNET!"));
                    }
                }
                else
                {
                    table.AddCell("");
                }
            }

            return new List<DocumentTable> { new DocumentTable("", table) };
        }

        private IEnumerable<ISafetyRelation> GetAllSafety()
        {

            SafetyRole role;
            if (Enum.TryParse(Role, true, out role))
            {
                var list = new List<ISafetyRelation>(Documentation.Project.Safeties
                    .Where(m => m.IncludeInManual && m.Role == role)
                    .ToList());

                foreach (var rel in Documentation.Project.Components)
                {
                    list.AddRange(rel.Component.Safeties
                        .Where(m => m.IncludeInManual && m.Role == role)
                        .Except(list));

                    var singleComponent = rel.Component as Component;

                    if (singleComponent != null && singleComponent.Series != null)
                    {
                        foreach (var rel2 in singleComponent.Series.Safeties)
                        {
                            list.AddRange(rel2.Component.Safeties
                                .Where(m => m.IncludeInManual && m.Role == role)
                                .Except(list));
                        }
                    }
                }

                list.Sort();
                return list.DistinctBy(rel => rel.Safety);
            }

            return new List<ISafetyRelation>();
        }
    }
}