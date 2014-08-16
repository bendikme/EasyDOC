using System;
using System.Collections.Generic;
using System.Linq;
using EasyDOC.Model;
using EasyDOC.Pdf.Interface;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace EasyDOC.Pdf.ManualTableGenerators
{
    class MaintenanceTableBuilder : ITableBuilder
    {
        public Documentation Documentation { get; set; }
        public string RootDirectory { get; set; }
        public string Titles { get; set; }
        public string Widths { get; set; }
        public string Info { get; set; }
        public string Info2 { get; set; }

        public bool HasMultipleTables
        {
            get { return false; }
        }

        public IReadOnlyDictionary<string, string> Parameters { get; set; }

        public IEnumerable<DocumentTable> GetTables()
        {
            var table = new PdfPTable(17)
            {
                WidthPercentage = 100
            };

            table.SetWidths(Widths.Split(',').Select(int.Parse).ToArray());

            var titles = Titles.Split(',');

            table.DefaultCell.Rowspan = 2;
            table.DefaultCell.BackgroundColor = new GrayColor(0.9f);

            table.AddCell(new Phrase(titles[0], PdfFonts.SmallBold));
            table.AddCell(new Phrase(titles[1], PdfFonts.SmallBold));
            table.AddCell(new Phrase(titles[2], PdfFonts.SmallBold));

            table.DefaultCell.Rotation = 90;
            table.DefaultCell.VerticalAlignment = Element.ALIGN_MIDDLE;
            table.AddCell(new Phrase(titles[3], PdfFonts.SmallBold));

            table.DefaultCell.Rotation = 0;
            table.DefaultCell.Rowspan = 1;
            table.DefaultCell.Colspan = 8;
            table.DefaultCell.HorizontalAlignment = Element.ALIGN_CENTER;
            table.AddCell(new Phrase(titles[4], PdfFonts.SmallBold));

            table.DefaultCell.Colspan = 2;
            table.DefaultCell.HorizontalAlignment = Element.ALIGN_LEFT;
            table.DefaultCell.VerticalAlignment = Element.ALIGN_TOP;
            table.AddCell(new Phrase(titles[5], PdfFonts.SmallBold));

            table.DefaultCell.Colspan = 1;
            table.DefaultCell.Rowspan = 2;
            table.AddCell(new Phrase(titles[6], PdfFonts.SmallBold));
            table.AddCell(new Phrase(titles[7], PdfFonts.SmallBold));
            table.AddCell(new Phrase(titles[8], PdfFonts.SmallBold));

            table.DefaultCell.Rotation = 90;
            table.DefaultCell.Rowspan = 1;
            table.DefaultCell.VerticalAlignment = Element.ALIGN_MIDDLE;
            table.AddCell(new Phrase(titles[9], PdfFonts.TinyBold));
            table.AddCell(new Phrase(titles[10], PdfFonts.TinyBold));
            table.AddCell(new Phrase(titles[11], PdfFonts.TinyBold));
            table.AddCell(new Phrase(titles[12], PdfFonts.TinyBold));
            table.AddCell(new Phrase(titles[13], PdfFonts.TinyBold));
            table.AddCell(new Phrase(titles[14], PdfFonts.TinyBold));
            table.AddCell(new Phrase(titles[15], PdfFonts.TinyBold));
            table.AddCell(new Phrase(titles[16], PdfFonts.TinyBold));

            table.AddCell(new Phrase(titles[17], PdfFonts.SmallBold));
            table.AddCell(new Phrase(titles[18], PdfFonts.SmallBold));

            table.HeaderRows = 2;

            table.DefaultCell.Rotation = 0;
            table.DefaultCell.BackgroundColor = BaseColor.WHITE;

            var maintenances = GetAllMaintenance();

            var enumerable = maintenances as IList<Maintenance> ?? maintenances.ToList();
            foreach (var maintenance in enumerable)
            {
                AddMaintenanceRow(maintenance, table);
            }

            table.DefaultCell.Colspan = 17;
            table.AddCell(" ");

            table.DefaultCell.Colspan = 4;
            table.DefaultCell.HorizontalAlignment = Element.ALIGN_LEFT;
            table.AddCell(new Phrase(Info, PdfFonts.Small));

            table.DefaultCell.Colspan = 1;
            table.DefaultCell.Rotation = 90;
            table.DefaultCell.HorizontalAlignment = Element.ALIGN_CENTER;
            table.DefaultCell.VerticalAlignment = Element.ALIGN_MIDDLE;

            table.AddCell(new Phrase("24", PdfFonts.Tiny));
            table.AddCell(new Phrase("168", PdfFonts.Tiny));
            table.AddCell(new Phrase("336", PdfFonts.Tiny));
            table.AddCell(new Phrase("720", PdfFonts.Tiny));
            table.AddCell(new Phrase("2160", PdfFonts.Tiny));
            table.AddCell(new Phrase("4320", PdfFonts.Tiny));
            table.AddCell(new Phrase("8640", PdfFonts.Tiny));
            table.AddCell(new Phrase(">8640", PdfFonts.Tiny));

            table.DefaultCell.Colspan = 5;
            table.AddCell("");

            table.DefaultCell.Rotation = 0;

            var vendors = enumerable
                .Select(m => m.Vendor)
                .Where(v => v != null)
                .Distinct()
                .OrderBy(v => v.Name);

            foreach (var vendor in vendors)
            {
                var abbreviation = GetVendorAbbreviation(vendor);

                table.DefaultCell.Colspan = 16;
                table.DefaultCell.HorizontalAlignment = Element.ALIGN_LEFT;
                table.AddCell(new Phrase(abbreviation + " = " + vendor.Name + "  " + Info2, PdfFonts.SmallBold));

                table.DefaultCell.Colspan = 1;
                table.DefaultCell.HorizontalAlignment = Element.ALIGN_CENTER;
                table.AddCell(GetVendorLink(vendor));
            }

            return new List<DocumentTable> { new DocumentTable("", table) };
        }

        private void AddMaintenanceRow(Maintenance m, PdfPTable table)
        {
            table.DefaultCell.HorizontalAlignment = Element.ALIGN_LEFT;
            table.DefaultCell.VerticalAlignment = Element.ALIGN_TOP;

            table.AddCell(new Phrase(m.Name, PdfFonts.Small));

            var phrase = new Phrase
            {
                new Phrase(m.Description, PdfFonts.Small)
            };

            if (m.Manual != null)
            {
                if (!string.IsNullOrWhiteSpace(m.Description))
                {
                    phrase.Add(Chunk.NEWLINE);
                    phrase.Add(Chunk.NEWLINE);
                }

                var chunk = new Chunk((m.ManualPage.HasValue ? string.Format("s. {0} ", m.ManualPage) : "") + m.Manual.GetPath(), PdfFonts.SmallLink);
                chunk.SetRemoteGoto(m.Manual.GetPath(), m.ManualPage.HasValue ? m.ManualPage.Value : 1);
                chunk.SetUnderline(0.5f, -2f);
                chunk.setLineHeight(13);
                phrase.Add(new Phrase(chunk));
            }

            table.AddCell(phrase);

            if (m.Image != null)
            {
                var fullName = string.Format(@"{0}\{1}", RootDirectory, m.Image.Path);

                try
                {
                    var image = Image.GetInstance(fullName);
                    image.ScaleToFit(Math.Min(100, image.Width), Math.Min(100, image.Height));

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

            table.AddCell("");

            table.DefaultCell.HorizontalAlignment = Element.ALIGN_CENTER;
            table.DefaultCell.VerticalAlignment = Element.ALIGN_MIDDLE;

            foreach (var interval in m.Intervals)
            {
                table.DefaultCell.BackgroundColor = interval ? BaseColor.GREEN : BaseColor.WHITE;
                table.AddCell(" ");
            }

            table.DefaultCell.BackgroundColor = BaseColor.WHITE;
            table.AddCell("");
            table.AddCell("");
            table.AddCell("");
            table.AddCell("");

            table.AddCell(GetVendorLink(m.Vendor));
        }

        private static Anchor GetVendorLink(Vendor vendor)
        {
            var anchor = new Anchor(new Phrase(vendor != null ? GetVendorAbbreviation(vendor) : "", PdfFonts.SmallBold));
            if (vendor != null)
            {
                anchor.Reference = vendor.Link;
            }
            return anchor;
        }

        private static string GetVendorAbbreviation(Vendor v)
        {
            return v.ShortName ?? v.Name.Substring(0, 3).ToUpper();
        }

        private IEnumerable<Maintenance> GetAllMaintenance()
        {
            var list = Documentation.Project.Maintenances
                .Where(m => m.IncludeInManual)
                .Select(m => m.Maintenance)
                .ToList();

            foreach (var rel in Documentation.Project.Components)
            {
                list.AddRange(rel.Component.Maintenances
                    .Where(m => m.IncludeInManual)
                    .Select(m => m.Maintenance)
                    .Except(list));

                var singleComponent = rel.Component as Component;

                if (singleComponent != null && singleComponent.Series != null)
                {
                    foreach (var rel2 in singleComponent.Series.Maintenances)
                    {
                        list.AddRange(rel2.Component.Maintenances
                            .Where(m => m.IncludeInManual)
                            .Select(m => m.Maintenance)
                            .Except(list));
                    }
                }
            }

            list.Sort(new MaintenanceSort());
            return list;
        }

        internal class MaintenanceSort : IComparer<Maintenance>
        {
            public int Compare(Maintenance m1, Maintenance m2)
            {
                var interval = GetLeastInterval(m1).CompareTo(GetLeastInterval(m2));
                return interval == 0 ? String.Compare(m1.Name, m2.Name, StringComparison.Ordinal) : interval;
            }

            private static int GetLeastInterval(Maintenance m)
            {
                for (int i = 0; i < m.Intervals.Count(); ++i)
                {
                    if (m.Intervals.ElementAt(i))
                    {
                        return i;
                    }
                }

                return m.Intervals.Count();
            }
        }
    }
}