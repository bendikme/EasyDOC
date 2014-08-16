using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using EasyDOC.DAL.DataAccess;
using EasyDOC.Model;
using Component = EasyDOC.Model.Component;

namespace EasyDOC.Pdf.Interface
{
    public class GenericParser : IOrderConfirmationParser
    {
        private readonly XElement _config;
        private readonly string _filename;
        private readonly string _root;

        public GenericParser(XElement config, string filename, string root)
        {
            _config = config;
            _filename = filename;
            _root = root;

            TypeDescriptor.AddAttributes(typeof(Employee), new TypeConverterAttribute(typeof(EmployeeConverter)));
            TypeDescriptor.AddAttributes(typeof(Unit), new TypeConverterAttribute(typeof(UnitConverter)));
        }

        public string Name
        {
            get { return _config.Attribute("name").Value; }
        }

        public OrderConfirmation ExtractOrderConfirmationData(IUnitOfWork uow)
        {
            var app = _config.Descendants("parameter")
                .Where(p => p.Attribute("name").Value == "PdfConverterPath")
                .Select(p => p.Attribute("value").Value)
                .FirstOrDefault();

            var items = _config.Descendants("items");
            var parsers = _config.Descendants("parser");

            var vendorName = _config.Attribute("validation").Value;
            var vendor = uow.VendorRepository.Get(v => v.Name == vendorName).FirstOrDefault();

            if (vendor == null)
            {
                vendor = new Vendor();
                uow.VendorRepository.Create(vendor);
            }

            var order = new OrderConfirmation
            {
                Vendor = vendor
            };

            foreach (var parser in parsers)
            {
                var tablefile = parser.Attribute("value").Value;
                var rowspace = parser.Attribute("rowspace").Value;

                var table = ParsePdfColumns(tablefile, app, rowspace);
                //var strategy = parser.Attribute("strategy").Value;

                foreach (var column in parser.Descendants("column"))
                {
                    var property = column.Attribute("name").Value;
                    var startRow = column.Attribute("startRowValue").Value;
                    var startOffset = Int32.Parse(column.Attribute("startRowOffset").Value);
                    var endRow = "";

                    var toEndAttr = column.Attribute("toEnd");
                    var toEnd = toEndAttr != null && Boolean.Parse(toEndAttr.Value);

                    if (column.Attribute("endRowValue") != null)
                    {
                        endRow = column.Attribute("endRowValue").Value;
                    }

                    var oneLine = endRow == "" && !toEnd;

                    var found = false;
                    var stringValue = "";

                    for (var columnIndex = 0; columnIndex < table[0].Length && !found; columnIndex++)
                    {
                        var fetching = false;
                        var firstLine = false;

                        for (var rowIndex = 0; rowIndex < table.Length; rowIndex++)
                        {
                            var cell = table[rowIndex][columnIndex];

                            if (!fetching && cell == startRow)
                            {
                                fetching = true;
                                firstLine = true;

                                if (oneLine)
                                {
                                    stringValue = table[rowIndex + startOffset][columnIndex];
                                    found = true;
                                    break;
                                }
                            }
                            else if (fetching && (cell != endRow || toEnd))
                            {
                                stringValue += (firstLine ? "" : "\n") + cell;
                                firstLine = false;
                            }
                            else if (fetching && cell == endRow)
                            {
                                found = true;
                                break;
                            }
                        }

                        if (toEnd && fetching)
                        {
                            found = true;
                        }
                    }

                    SetValueOfProperty(stringValue.Trim(), property, order);
                }
            }


            foreach (var item in items)
            {
                var tablefile = item.Attribute("value").Value;
                var rowspace = item.Attribute("rowspace").Value;
                var checkColumn = Int32.Parse(item.Attribute("startFetchCheckColumn").Value);
                var regex = new Regex(item.Attribute("startFetchRegex").Value);

                var table = ParsePdfColumns(tablefile, app, rowspace);

                var rowIndex = 0;
                while (rowIndex < table.Length)
                {
                    var found = false;

                    var orderItem = new OrderConfirmationItem
                    {
                        Component = new Component()
                    };

                    if (regex.IsMatch(table[rowIndex][checkColumn]))
                    {
                        var first = true;
                        found = true;

                        foreach (var row in item.Descendants("row"))
                        {
                            if (!first && regex.IsMatch(table[rowIndex][checkColumn]))
                            {
                                break;
                            }

                            first = false;

                            var columnIndex = 0;

                            foreach (var column in row.Descendants("column"))
                            {
                                var skipAttr = column.Attribute("skip");
                                if (skipAttr != null && Boolean.Parse(skipAttr.Value))
                                {
                                    ++columnIndex;
                                    continue;
                                }

                                var stringValue = "";

                                var rowSpanAttr = column.Attribute("rowspan");
                                var colSpanAttr = column.Attribute("colspan");
                                var avoidRegexAttr = column.Attribute("avoidWhenMatchesRegex");

                                var rowSpan = rowSpanAttr == null ? 1 : int.Parse(rowSpanAttr.Value);
                                var colSpan = colSpanAttr == null ? 1 : int.Parse(colSpanAttr.Value);

                                var previousColumnIndex = columnIndex;
                                for (var i = 0; i < rowSpan; ++i)
                                {
                                    var currentCell = "";
                                    var currentLine = "";

                                    for (var j = 0; j < colSpan; ++j)
                                    {
                                        currentCell = table[rowIndex][columnIndex++].Trim();

                                        if (!Regex.IsMatch(currentCell, avoidRegexAttr != null ? avoidRegexAttr.Value : "^$"))
                                        {
                                            currentLine += currentCell;
                                        }
                                    }

                                    if (i > 0 && i < rowSpan && currentLine.Length > 0)
                                    {
                                        stringValue += "\n";
                                    }

                                    stringValue += currentLine;

                                    if (rowSpan > 1)
                                    {
                                        columnIndex = previousColumnIndex;

                                        if (regex.IsMatch(table[++rowIndex][checkColumn]))
                                        {
                                            --rowIndex;
                                            break;
                                        }
                                    }
                                }

                                var property = column.Attribute("name").Value;
                                SetValueOfProperty(stringValue, property, orderItem);
                            }

                            ++rowIndex;
                        }
                    }
                    else
                    {
                        ++rowIndex;
                    }

                    if (found) order.Items.Add(orderItem);
                }
            }

            if (uow.OrderConfirmationRepository.Get(o => o.OrderNumber == order.OrderNumber && o.VendorId == order.VendorId).Any())
            {
                throw new Exception("Order number already in database");
            }

            uow.OrderConfirmationRepository.Create(ReplaceExistingComponents(order, uow));
            return order;
        }

        private static OrderConfirmation ReplaceExistingComponents(OrderConfirmation order, IUnitOfWork uow)
        {
            foreach (var item in order.Items)
            {
                var currentItem = item;
                var component = uow.SingleComponentRepository
                    .Get(c => c.Name == currentItem.Component.Name && c.VendorId == currentItem.Component.VendorId)
                    .FirstOrDefault();

                if (component != null)
                {
                    item.Component = component;
                }
                else
                {
                    uow.SingleComponentRepository.Create(item.Component);
                }
            }

            return order;
        }

        private static void SetValueOfProperty(string stringValue, string name, object item)
        {
            var path = name.Split('.');
            var type = item.GetType();
            PropertyInfo propertyInfo = null;

            // Find the actual property to set
            foreach (var prop in path)
            {
                if (propertyInfo != null)
                    item = propertyInfo.GetValue(item);

                propertyInfo = type.GetProperty(prop);
                type = propertyInfo.PropertyType;
            }

            var converter = TypeDescriptor.GetConverter(type);
            if (converter.CanConvertFrom(typeof(string)))
            {
                if (type == typeof(Decimal))
                {
                    stringValue = RemoveNumberFormatting(stringValue);
                }

                if (propertyInfo != null)
                {
                    propertyInfo.SetValue(item, converter.ConvertFrom(stringValue));
                }
            }
        }

        private static string RemoveNumberFormatting(string value)
        {
            var indexOfDot = value.IndexOf('.');
            var indexOfComma = value.IndexOf(',');

            if (indexOfDot > -1 && indexOfComma > -1)
                value = value.Replace(indexOfDot < indexOfComma ? "." : ",", "");

            value = value.Replace(".", ",");
            return value.Length == 0 ? "0" : value;
        }

        public IEnumerable<OrderConfirmationItem> ExtractTabularDataFromPdf()
        {
            return null;
        }

        public string[][] ParsePdfColumns(string pdfConverterRules, string processFilename, string rowspace)
        {
            var arguments = string.Format(@"-R""{0}\{1}"" -F""{2}"" -O""{2}.csv"" -S{3}", _root, pdfConverterRules, _filename, rowspace);
            int exitCode = -1;

            var start = new ProcessStartInfo
            {
                Arguments = arguments,
                FileName = processFilename,
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true,
                UseShellExecute = false
            };

            // Run the external process & wait for it to finish
            using (var proc = Process.Start(start))
            {
                if (proc != null)
                {
                    proc.WaitForExit(5000);
                    exitCode = proc.ExitCode;
                }
            }

            if (exitCode != 0)
            {
                return null;
            }

            var filename = string.Format(@"{0}.csv", _filename);
            var rawLines = System.IO.File.ReadAllLines(filename, Encoding.Default);
            var table = rawLines.Select(line => line.Split(new[] { @",""" }, StringSplitOptions.None)).ToArray();

            foreach (var row in table)
            {
                for (int j = 0; j < row.Length; ++j)
                {
                    row[j] = RemoveQuotationsAndTrim(row[j]);
                }
            }

            return table;
        }

        protected string RemoveQuotationsAndTrim(string input)
        {
            input = input.StartsWith("\"") ? input.Substring(1) : input;
            input = (input.EndsWith("\"") ? input.Substring(0, input.Length - 1) : input).Trim();
            return input.Replace("\"\"", "\"");
        }

        public string VendorName { get; set; }
    }
}