using System;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using EasyDOC.Model;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using Type = System.Type;

namespace EasyDOC.Pdf.Interface
{
    public static class OrderConfirmationFactory
    {
        public static IOrderConfirmationParser GetParser(string root, File file)
        {
            var doc = XDocument.Load(root + @"\Config\Orders.xml");
            IOrderConfirmationParser instance = null;

            var filename = string.Format("{0}{1}", root, file.GetPath());
            var reader = new PdfReader(filename);

            var strategy = new LocationTextExtractionStrategy();
            var text = PdfTextExtractor.GetTextFromPage(reader, 1, strategy);

            // Replace non-breaking spaces with normal spaces
            text = text.Replace('\u00A0', ' ');

            var nodes = doc.Descendants("order");
            var actualNode = nodes.FirstOrDefault(n =>
            {
                var vendor = n.Descendants("vendor").FirstOrDefault();
                if (vendor != null)
                {
                    var vendorName = vendor.Attribute("value").Value;
                    var hint = vendor.Attribute("hint");

                    return text.Contains(vendorName) || (hint != null && text.Contains(hint.Value));
                }

                return false;
            });

            if (actualNode != null)
            {
                var vendorName = actualNode.Descendants("vendor").First().Attribute("value").Value;
                var className = actualNode.Attribute("class");
                if (className == null) throw new ArgumentException("No order confirmation parser found for vendor " + vendorName);

                var type = Type.GetType(className.Value, true);
                instance = Activator.CreateInstance(type, root, file.GetPath()) as IOrderConfirmationParser;
                instance.VendorName = vendorName;

                foreach (var arg in actualNode.Descendants("parameter"))
                {
                    var name = arg.Attribute("name").Value;
                    var value = arg.Attribute("value").Value;

                    var property = type.GetProperty(name);
                    if (property != null)
                    {
                        if (name.StartsWith("PdfConverterRules") && arg.Attribute("extend") != null && Boolean.Parse(arg.Attribute("extend").Value))
                        {
                            property.SetValue(instance, TableFileExtender.ExtendFile(root, value, 25));
                        }
                        else
                        {
                            property.SetValue(instance, value);
                        }
                    }
                    else
                    {
                        throw new ArgumentException("Parameter " + name + " not found on type " + type.Name);
                    }
                }
            }

            return instance;
        }
    }
}