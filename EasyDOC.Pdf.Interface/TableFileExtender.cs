using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace EasyDOC.Pdf.Interface
{
    public class TableFileExtender
    {
        public static string ExtendFile(string root, string filename, int pages)
        {
            try
            {
                var doc = XDocument.Load(root + "\\" + filename);

                var grids = doc.Element("Grids");

                XElement nodeToClone;
                var startAt = 1;

                if (grids.Elements("Grid").Count() > 1)
                {
                    nodeToClone = new XElement((XElement)grids.Nodes().ElementAt(1));
                    var firstNode = new XElement((XElement) grids.FirstNode);

                    grids.RemoveAll();
                    grids.Add(firstNode);
                    startAt = 2;
                }
                else
                {
                    nodeToClone = new XElement((XElement)grids.FirstNode);
                    grids.RemoveAll();
                }


                for (var i = startAt; i <= pages; ++i)
                {
                    var element = new XElement(nodeToClone);
                    var attr = element.Attribute("pageIndex");
                    attr.Value = i.ToString(CultureInfo.InvariantCulture);

                    grids.Add(element);
                }

                var file = filename + "_" + pages;

                var settings = new XmlWriterSettings
                {
                    OmitXmlDeclaration = true
                };

                using (var writer = XmlWriter.Create(root + "\\" + file, settings))
                {
                    doc.Save(writer);
                }

                return file;

            }
            catch (IOException e)
            {

            }

            return null;
        }
    }
}
