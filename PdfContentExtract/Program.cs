using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PdfContentExtract
{
    class Program
    {
        static void Main(string[] args)
        {
            var file = @"c:\users\bendike\desktop\Ordrebekreftelse __115723.PDF";

            var reader = new PdfReader(file);
            var pages = new List<String>();

            for (int i = 0; i < reader.NumberOfPages; i++)
            {
                var textFromPage = Encoding.UTF8.GetString(Encoding.Convert(Encoding.Default, Encoding.UTF8, reader.GetPageContent(i + 1)));
                pages.Add(GetDataConvertedData(textFromPage));
            }


        }

        private static string GetDataConvertedData(string textFromPage)
        {
            var texts = textFromPage.Split(new[] { "\n" }, StringSplitOptions.None)
                                .Where(text => text.Contains("Tj")).ToList();

            return texts.Aggregate(string.Empty, (current, t) => current +
                       t.TrimStart('(')
                        .TrimEnd('j')
                        .TrimEnd('T')
                        .TrimEnd(')'));
        }
    }
}
