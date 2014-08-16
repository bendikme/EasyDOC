using System.Collections.Generic;
using System.Linq;
using EasyDOC.Model;
using iTextSharp.text;

namespace EasyDOC.Pdf.ManualTableGenerators
{
    class DocxToPdf
    {
        private readonly Documentation _doc;
        private readonly string _root;
        private Stack<ITextElementArray> _contexts;

        public DocxToPdf(Documentation doc, string root)
        {
            _doc = doc;
            _root = root;
        }

        public IElement Parse(string filename)
        {
            _contexts = new Stack<ITextElementArray>();
            _contexts.Push(new Paragraph());

            return _contexts.Last();
        }
    }
}