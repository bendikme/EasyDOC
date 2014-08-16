using iTextSharp.text;

namespace EasyDOC.Pdf.ManualTableGenerators
{
    internal class FormattingOptions
    {
        public bool Bold { get; set; }
        public bool Underlined { get; set; }
        public bool Italic { get; set; }
        public bool Link { get; set; }
        public List List { get; set; }
        public bool ListItem { get; set; }
    }
}