using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using EasyDOC.Pdf.Interface;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PdfParseTest
{
    [TestClass]
    public class TableFileExtenderTest
    {
        [TestMethod]
        public void ValidFileReturned()
        {
            const string root = @"C:\Users\bendike\Documents\Visual Studio 2012\Projects\EasyDOC\EasyDOC.Api\";
            var file = TableFileExtender.ExtendFile(root, @"PdfConverterRules\Intralox-MDD.tbl", 20);

            Assert.IsNotNull(file);
            Assert.IsTrue(System.IO.File.Exists(root + "\\" + file));
        }


        [TestMethod]
        public void NodesExtendedCorrectly()
        {
            const string root = @"C:\Users\bendike\Documents\Visual Studio 2012\Projects\EasyDOC\EasyDOC.Api\";
            var file = TableFileExtender.ExtendFile(root, @"PdfConverterRules\Intralox-MDD.tbl", 20);

            var doc = XDocument.Load(root + "\\" + file);
            var nodes = doc.Descendants("Grid");

            for (var i = 1; i <= 20; ++i)
            {
                Assert.AreEqual(nodes.ElementAt(i - 1).Attribute("pageIndex").Value, i.ToString(CultureInfo.InvariantCulture));
            }
        }
    }
}
