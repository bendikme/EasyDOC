using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using EasyDOC.Model;
using EasyDOC.Pdf.Interface;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Image = iTextSharp.text.Image;

namespace EasyDOC.Pdf.ManualTableGenerators
{
    class ComponentTableBuilder : ITableBuilder
    {
        public Documentation Documentation { get; set; }
        public string RootDirectory { get; set; }
        public string Titles { get; set; }
        public string Widths { get; set; }
        public string Info { get; set; }

        public bool HasMultipleTables
        {
            get { return false; }
        }

        public IReadOnlyDictionary<string, string> Parameters { get; set; }

        public IEnumerable<DocumentTable> GetTables()
        {
            var table = new PdfPTable(8)
            {
                WidthPercentage = 100
            };

            table.SetWidths(Widths.Split(',').Select(int.Parse).ToArray());

            table.DefaultCell.Padding = 3.0f;
            table.DefaultCell.BackgroundColor = new GrayColor(0.9f);

            var titles = Titles.Split(',');

            table.AddCell(new Phrase(titles[1], PdfFonts.Bold));
            table.AddCell(new Phrase(titles[0], PdfFonts.Bold));
            table.AddCell(new Phrase(titles[2], PdfFonts.Bold));
            table.AddCell(new Phrase(titles[3], PdfFonts.Bold));
            table.AddCell(new Phrase(titles[4], PdfFonts.Bold));
            table.AddCell(new Phrase(titles[5], PdfFonts.Bold));
            table.AddCell(new Phrase(titles[6], PdfFonts.Bold));
            table.AddCell(new Phrase(Documentation.Project.ProjectNumber, PdfFonts.Bold));

            table.HeaderRows = 1;
            table.DefaultCell.BackgroundColor = BaseColor.WHITE;

            var vendors = new List<Vendor>();

            foreach (var rel in Documentation.Project.Components.Where(rel => rel.Component is Component && rel.IncludeInManual)
                .OrderBy(rel => rel.Component.Vendor != null ? rel.Component.Vendor.Name : "")
                .ThenBy(rel => rel.Component.Category != null ? rel.Component.Category.Name : "")
                .ThenBy(rel => rel.Component.Name))
            {
                table.AddCell(GetVendorLink(rel.Component.Vendor));
                table.AddCell(new Phrase(rel.Component.Category != null ? GetCategoryPath(rel.Component.Category) : "", PdfFonts.Normal));
                table.AddCell(new Phrase(rel.Component.Name, PdfFonts.Normal));
                table.AddCell(new Phrase(rel.Component.Description, PdfFonts.Normal));

                if (rel.Component.Image != null)
                {
                    var fullName = string.Format(@"{0}\{1}", RootDirectory, rel.Component.Image.Path);

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

                table.AddCell(new Phrase(rel.Count.ToString(CultureInfo.InvariantCulture), PdfFonts.Normal));
                table.AddCell(new Phrase(rel.SpareParts.ToString(CultureInfo.InvariantCulture), PdfFonts.Normal));
                table.AddCell(new Phrase(rel.Info, PdfFonts.Normal));

                if (rel.Component.Vendor != null)
                {
                    vendors.Add(rel.Component.Vendor);
                }
            }

            vendors = vendors.Distinct().OrderBy(v => v.Name).ToList();

            foreach (var vendor in vendors)
            {
                var abbreviation = GetVendorAbbreviation(vendor);

                table.DefaultCell.Colspan = 7;
                table.DefaultCell.HorizontalAlignment = Element.ALIGN_LEFT;
                table.AddCell(new Phrase(abbreviation + " = " + vendor.Name + "  " + Info, PdfFonts.SmallBold));

                table.DefaultCell.Colspan = 1;
                table.DefaultCell.HorizontalAlignment = Element.ALIGN_CENTER;
                table.AddCell(GetVendorLink(vendor));
            }

            return new List<DocumentTable> { new DocumentTable("", table) };
        }

        private static string GetVendorAbbreviation(Vendor v)
        {
            return v.ShortName ?? v.Name.Substring(0, 3).ToUpper();
        }

        private string GetCategoryPath(Category category)
        {
            var path = category.Name;
            var parent = category.ParentCategory;
            while (parent != null && parent.ParentCategory != null)
            {
                path = parent.Name + " - " + path;
                parent = parent.ParentCategory;
            }

            return path;
        }

        private static Anchor GetVendorLink(Vendor vendor)
        {
            var anchor = new Anchor(new Phrase(vendor != null ? GetAbbreviation(vendor) : "", PdfFonts.SmallBold));
            if (vendor != null)
            {
                anchor.Reference = vendor.Link;
            }
            return anchor;
        }

        private static string GetAbbreviation(Vendor v)
        {
            return v.ShortName ?? v.Name.Substring(0, 3).ToUpper();
        }

    }
}