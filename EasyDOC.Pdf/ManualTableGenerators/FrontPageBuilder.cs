using System;
using EasyDOC.Model;
using EasyDOC.Utilities;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace EasyDOC.Pdf.ManualTableGenerators
{
    class FrontPageBuilder
    {
        private readonly string _root;
        private readonly Documentation _doc;

        public FrontPageBuilder(Documentation doc, string root)
        {
            _doc = doc;
            _root = root;
        }

        public IElement GetContent()
        {
            var paragraph = new Paragraph();
            var table = new PdfPTable(1)
            {
                WidthPercentage = 100
            };

            table.DefaultCell.Border = 0;
            table.DefaultCell.Padding = 2.0f;
            table.DefaultCell.FixedHeight = 25;
            table.DefaultCell.HorizontalAlignment = Element.ALIGN_CENTER;
            table.DefaultCell.VerticalAlignment = Element.ALIGN_MIDDLE;

            table.AddCell(new Phrase("BRUKSANVISNING", PdfFonts.Header1));
            table.AddCell(new Phrase(_doc.Name, PdfFonts.Header2));


            if (_doc.Image != null)
            {
                var src = _doc.Image.GetPath();
                if (!EasyUtilities.IsAbsoluteUrl(src))
                {
                    src = _root + "/" + src;
                }

                try
                {
                    var image = Image.GetInstance(src);
                    image.ScaleToFit(Math.Min(460, image.Width), Math.Min(575, image.Height));

                    var cell = new PdfPCell(image)
                    {
                        Border = 0,
                        FixedHeight = 575,
                        Padding = 0,
                        HorizontalAlignment = Element.ALIGN_CENTER,
                        VerticalAlignment = Element.ALIGN_MIDDLE
                    };

                    table.AddCell(cell);
                }
                catch
                {
                    table.AddCell(new Phrase("BILDET " + src + " IKKE FUNNET!"));
                }
            }
            else
            {
                var cell = new PdfPCell
                {
                    Border = 0,
                    FixedHeight = 500
                };

                table.AddCell(cell);
            }

            table.DefaultCell.FixedHeight = 16;
            table.AddCell(new Phrase(_doc.Project.ProjectNumber, PdfFonts.Header3));

            if (_doc.Project.Customer != null)
            {
                table.AddCell(new Phrase(_doc.Project.Customer.Name, PdfFonts.Header3));
            }

            paragraph.Add(table);
            return paragraph;
        }

    }
}