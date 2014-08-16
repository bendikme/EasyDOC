using System;
using System.Collections.Generic;
using System.Linq;
using EasyDOC.Model;
using EasyDOC.Pdf.Interface;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace EasyDOC.Pdf.ManualTableGenerators
{
    class AttachmentsTableBuilder : ITableBuilder
    {
        public Documentation Documentation { get; set; }
		public string Role { get; set; }

		public string Titles { get; set; }
		public string Widths { get; set; }

        public bool HasMultipleTables
        {
            get { return false; }
        }

        public IReadOnlyDictionary<string, string> Parameters { get; set; }

        public IEnumerable<DocumentTable> GetTables()
        {
            var table = new PdfPTable(4)
            {
                WidthPercentage = 100
            };

            var titles = Titles.Split(',');

			table.SetWidths(Widths.Split(',').Select(int.Parse).ToArray());

            table.DefaultCell.BackgroundColor = new GrayColor(0.9f);
            table.DefaultCell.VerticalAlignment = Element.ALIGN_MIDDLE;

            table.AddCell(new Phrase(titles[0], PdfFonts.Bold));
            table.AddCell(new Phrase(titles[1], PdfFonts.Bold));
            table.AddCell(new Phrase(titles[2], PdfFonts.Bold));
            table.AddCell(new Phrase(titles[3], PdfFonts.Bold));

            table.DefaultCell.BackgroundColor = BaseColor.WHITE;
            table.DefaultCell.VerticalAlignment = Element.ALIGN_TOP;

            var attachments = GetAttachments(Role).ToList();
            attachments.Sort(delegate(IFileRelation f1, IFileRelation f2)
            {
                if (f1.Vendor != null && f2.Vendor != null)
                {
                    return f1.Vendor.Name == f2.Vendor.Name ?
                        String.Compare(f1.Name, f2.Name, StringComparison.Ordinal) :
                        String.Compare(f1.Vendor.Name, f2.Vendor.Name, StringComparison.Ordinal);
                }

                if (f1.Vendor == null && f2.Vendor != null)
                {
                    return -1;
                }

                return 1;
            });

            foreach (var attachment in attachments)
            {
                AddAttachmentRow(attachment, table);
            }

            return new List<DocumentTable> { new DocumentTable("", table) };
        }

        private static void AddAttachmentRow(IFileRelation rel, PdfPTable table)
        {

            if (rel.File != null)
            {
                table.AddCell(GetVendorLink(rel.Vendor));
                table.AddCell(new Phrase(rel.Name, PdfFonts.Normal));

                var path = rel.File.GetPath();
                var chunk = new Chunk(path, PdfFonts.Link);
                chunk.SetRemoteGoto(path, 1);
                chunk.SetUnderline(0.5f, -2f);
                chunk.setLineHeight(15);
                table.AddCell(new Phrase(chunk));

                table.DefaultCell.HorizontalAlignment = Element.ALIGN_CENTER;
                table.DefaultCell.VerticalAlignment = Element.ALIGN_MIDDLE;

                table.AddCell(new Phrase(rel.IncludedPrintedVersion ? "Ja" : "Nei", PdfFonts.Normal));

                table.DefaultCell.HorizontalAlignment = Element.ALIGN_LEFT;
                table.DefaultCell.VerticalAlignment = Element.ALIGN_TOP;
            }
        }

        private static Phrase GetVendorLink(Vendor vendor)
        {
            if (vendor != null && !string.IsNullOrWhiteSpace(vendor.Link))
            {
                var anchor = new Anchor(new Phrase(vendor.Name, PdfFonts.Link))
                {
                    Reference = vendor.Link
                };

                return anchor;
            }

            return new Phrase(vendor != null ? vendor.Name : " - ", PdfFonts.Normal);
        }

        private IEnumerable<IFileRelation> GetAttachments(string role)
        {
            var list = new List<IFileRelation>();
            list.AddRange(Documentation.Project.Files
                .Where(m => m.IncludeInManual && m.Role.ToString() == role)
                .ToList());

            foreach (var rel in Documentation.Project.Components)
            {
                list.AddRange(rel.Component.Files
                    .Where(m => m.IncludeInManual && m.Role.ToString() == role)
                    .Except(list));

                var singleComponent = rel.Component as Component;

                if (singleComponent != null && singleComponent.Series != null)
                {
                    foreach (var rel2 in singleComponent.Series.Files)
                    {
                        list.AddRange(rel2.Component.Files
                            .Where(m => m.IncludeInManual && m.Role.ToString() == role)
                            .Except(list));
                    }
                }
            }

            list.Sort();
            return list;
        }
    }
}