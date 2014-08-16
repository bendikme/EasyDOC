using iTextSharp.text;

namespace EasyDOC.Pdf.ManualTableGenerators
{
    class PdfFonts
    {
        public static Font Header1 = new Font(Font.FontFamily.HELVETICA, 16, Font.BOLD);
        public static Font Header2 = new Font(Font.FontFamily.HELVETICA, 14, Font.BOLD);
        public static Font Header3 = new Font(Font.FontFamily.HELVETICA, 12, Font.BOLD);
        public static Font Header4 = new Font(Font.FontFamily.HELVETICA, 11, Font.BOLD);
        public static Font Header5 = new Font(Font.FontFamily.HELVETICA, 11, Font.NORMAL);

        public static Font Large = new Font(Font.FontFamily.HELVETICA, 14, Font.NORMAL);
        public static Font Normal = new Font(Font.FontFamily.HELVETICA, 12, Font.NORMAL);
        public static Font Small = new Font(Font.FontFamily.HELVETICA, 10, Font.NORMAL);
        public static Font Tiny = new Font(Font.FontFamily.HELVETICA, 9, Font.NORMAL);

        public static Font Link = new Font(Font.FontFamily.HELVETICA, 12, Font.NORMAL, BaseColor.BLUE);
        public static Font SmallLink = new Font(Font.FontFamily.HELVETICA, 10, Font.NORMAL, BaseColor.BLUE);

        public static Font Bold = new Font(Font.FontFamily.HELVETICA, 12, Font.BOLD);
        public static Font SmallBold = new Font(Font.FontFamily.HELVETICA, 10, Font.BOLD);
        public static Font TinyBold = new Font(Font.FontFamily.HELVETICA, 9, Font.BOLD);

        public static Font Italic = new Font(Font.FontFamily.HELVETICA, 12, Font.ITALIC);
        public static Font Underlined = new Font(Font.FontFamily.HELVETICA, 12, Font.UNDERLINE);
    }
}