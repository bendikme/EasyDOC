using iTextSharp.text.pdf;

namespace EasyDOC.Pdf.Interface
{
    public class DocumentTable
    {
        public DocumentTable(string title, PdfPTable table)
        {
            Table = table;
            Title = title;
        }

        public string Title { get; set; }
        public PdfPTable Table { get; private set; }
    }
}