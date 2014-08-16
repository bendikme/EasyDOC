using System.Collections.Generic;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace EasyDOC.Pdf.ManualTableGenerators
{
    class PdfEvents : PdfPageEventHelper
    {
        public readonly List<PdfMenuItem> Toc = new List<PdfMenuItem>();

        private bool _firstChapter = true;
        private readonly Image _logo;

        public PdfEvents(Document dummy, string root)
        {
            try
            {
                _logo = Image.GetInstance(root + "/Content/images/logo.png");
                _logo.SetAbsolutePosition(PageSize.A4.Width - 150, PageSize.A4.Height - 60);
                _logo.ScaleToFit(80, 80);
            }
            catch
            {
                // TODO: add to error list
            }
        }

        public override void OnChapter(PdfWriter writer, Document document, float paragraphPosition, Paragraph title)
        {
            var p = new Paragraph(title.Content.ToUpper(), new Font(Font.FontFamily.HELVETICA, 12, Font.BOLD))
            {
                SpacingAfter = 10,
                SpacingBefore = 10
            };

            if (_firstChapter)
            {
                _firstChapter = false;
                var header = new Paragraph("INNHOLD", new Font(Font.FontFamily.HELVETICA, 16, Font.BOLD))
                {
                    SpacingBefore = 14
                };

                header.Add(p);
            }

            Toc.Add(new PdfMenuItem { Level = 0, Title = title.Content.ToUpper(), Page = writer.CurrentPageNumber });
        }

        public override void OnSection(PdfWriter writer, Document document, float paragraphPosition, int depth, Paragraph title)
        {
            title = new Paragraph(title.Content.ToUpper())
            {
                IndentationLeft = 10 * depth
            };
            Toc.Add(new PdfMenuItem { Level = depth, Title = title.Content.ToUpper(), Page = writer.CurrentPageNumber });
        }

        public override void OnEndPage(PdfWriter writer, Document document)
        {
            if (document.PageSize.Width != PageSize.A4.Width)
            {
                _logo.SetAbsolutePosition(PageSize.A4.Height - 150, PageSize.A4.Width - 60);
            }
            else
            {
                _logo.SetAbsolutePosition(PageSize.A4.Width - 150, PageSize.A4.Height - 60);
            }
            document.Add(_logo);
        }
    }
}