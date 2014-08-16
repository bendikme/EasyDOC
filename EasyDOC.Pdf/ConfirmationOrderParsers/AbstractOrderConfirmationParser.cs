using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using EasyDOC.DAL.DataAccess;
using EasyDOC.Model;
using EasyDOC.Pdf.Interface;
using Component = EasyDOC.Model.Component;

namespace EasyDOC.Pdf.ConfirmationOrderParsers
{
    public abstract class AbstractOrderConfirmationParser : IOrderConfirmationParser
    {
        private readonly string _rootDirectory;
        protected readonly string Filename;
        public string PdfConverterPath { get; set; }
        public string RowSpace { get; set; }
        public string VendorName { get; set; }
        protected string DateFormat = "";

        protected AbstractOrderConfirmationParser(string rootDirectory, string filename)
        {
            _rootDirectory = rootDirectory;
            Filename = filename;

            TypeDescriptor.AddAttributes(typeof(Employee), new TypeConverterAttribute(typeof(EmployeeConverter)));
            TypeDescriptor.AddAttributes(typeof(Unit), new TypeConverterAttribute(typeof(UnitConverter)));
        }

        public string[][] ParsePdfColumns(string pdfConverterRules)
        {
            var arguments = string.Format(@"-R""{0}\{1}"" -F""{0}\{2}"" -O""{0}\{2}.csv"" -S{3}", _rootDirectory, pdfConverterRules, Filename, RowSpace);
            int exitCode = -1;

            var start = new ProcessStartInfo
            {
                Arguments = arguments,
                FileName = PdfConverterPath,
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

            var filename = string.Format(@"{0}\{1}.csv", _rootDirectory, Filename);
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

        public abstract OrderConfirmation ExtractOrderConfirmationData(IUnitOfWork uow);

        protected void ExtractFromTableRowDown(IReadOnlyList<List<Tuple<string, string>>> properties, string[][] table, OrderConfirmation order)
        {
            for (var colIndex = 0; colIndex < properties.Count; ++colIndex)
            {
                var rowCount = properties[colIndex].Count;

                for (var rowIndex = 0; rowIndex < rowCount; ++rowIndex)
                {
                    var property = properties[colIndex][rowIndex].Item2;

                    if (!string.IsNullOrWhiteSpace(property))
                    {
                        var start = properties[colIndex][rowIndex].Item1;
                        var end = rowIndex < rowCount - 1 ? properties[colIndex][rowIndex + 1].Item1 : null;

                        var result = CombineColumnRows(colIndex, start, end, table);
                        SetValueOfProperty(result, property, order);
                    }
                }
            }
        }

        protected void ExtractFromTableColumnLeftToRight(List<List<Tuple<string, string>>> properties, string[][] table, int titleColumn, int tableColumn, OrderConfirmation order)
        {
            for (var rowIndex = 0; rowIndex < properties[titleColumn].Count; ++rowIndex)
            {
                var property = properties[titleColumn][rowIndex].Item2;

                if (!string.IsNullOrWhiteSpace(property))
                {
                    var key = properties[titleColumn][rowIndex].Item1;
                    var result = GetValueOfRow(key, table, tableColumn);
                    SetValueOfProperty(result, property, order);
                }
            }
        }

        protected void ExtractFromTableColumnLeftToRight(List<List<Tuple<string, string>>> properties, string[][] table, int column, OrderConfirmation order)
        {
            for (var rowIndex = 0; rowIndex < properties[column].Count; ++rowIndex)
            {
                var property = properties[column][rowIndex].Item2;

                if (!string.IsNullOrWhiteSpace(property))
                {
                    var key = properties[column][rowIndex].Item1;
                    var result = GetValueOfRow(key, table, column, 0);

                    var index = result.IndexOf(key, StringComparison.Ordinal) + key.Length;
                    var valuePart = index < result.Length - 1 ? result.Substring(index) : result.Substring(0, index - key.Length);
                    SetValueOfProperty(valuePart.Trim(), property, order);
                }
            }
        }

        protected void ExtractFromTableColumnLeftToRight(List<List<Tuple<string, Expression<Func<object, object>>>>> properties, string[][] table, int column, OrderConfirmation order)
        {
            for (var rowIndex = 0; rowIndex < properties[column].Count; ++rowIndex)
            {
                var property = properties[column][rowIndex].Item2;

                var key = properties[column][rowIndex].Item1;
                var result = GetValueOfRow(key, table, column, 0);

                var index = result.IndexOf(key, StringComparison.Ordinal) + key.Length;
                var valuePart = index < result.Length - 1 ? result.Substring(index) : result.Substring(0, index - key.Length);
                SetValueOfProperty(valuePart.Trim(), property, order);
            }
        }

        private static string GetValueOfRow(string key, IEnumerable<string[]> table, int tableColumn, int offset = 1)
        {
            foreach (var row in table)
            {
                if (RemoveTrailingDots(row[tableColumn]).Contains(key))
                {
                    return RemoveTrailingDots(row[tableColumn + offset]);
                }
            }

            return "";
        }

        private static string RemoveTrailingDots(string input)
        {
            const string trailingDots = @"(?:\s|\.)*(.*)";
            var match = Regex.Match(input, trailingDots);
            return match.Groups.Count > 1 ? match.Groups[1].Value.Trim() : input;
        }

        protected static string CombineRowsBetween(string cellBefore, string cellAfter, IEnumerable<string[]> properties)
        {
            var capture = false;
            var value = "";
            var index = 0;

            foreach (var row in properties)
            {
                for (var cellIndex = 0; cellIndex < row.Length; cellIndex++)
                {
                    if (capture && index != cellIndex)
                    {
                        continue;
                    }

                    var cellValue = row[cellIndex];

                    if (cellValue.StartsWith(cellBefore))
                    {
                        capture = true;
                        index = cellIndex;
                    }
                    else if (cellValue.StartsWith(cellAfter))
                    {
                        return value;
                    }
                    else if (capture)
                    {
                        var newValue = cellValue.Trim();
                        value += (value.Length > 0 && newValue.Length > 0 ? "\n" : "") + newValue;
                    }
                }
            }

            return value;
        }

        protected static string CombineRowsBetween(int firstCell, int lastCell, int column, string[][] properties)
        {
            var value = "";

            for (var row = firstCell; row < lastCell && row < properties.Length; ++row)
            {
                var cellValue = properties[row][column].Trim();
                value += (value.Length > 0 && cellValue.Length > 0 ? "\n" : "") + cellValue;
            }

            return value;
        }

        protected static string FindCellStartingWith(string search, IEnumerable<string[]> properties)
        {
            foreach (var cell in from row in properties
                                 from cell in row
                                 where cell.StartsWith(search)
                                 select cell)
            {
                return cell;
            }

            return "";
        }

        private static string CombineColumnRows(int colIndex, string start, string end, IEnumerable<string[]> table)
        {
            var result = "";
            var fetch = false;
            var first = true;

            if (end == null)
            {
                end = "-----";
            }

            foreach (string[] row in table)
            {
                var cellValue = row[colIndex].Trim();

                if (!fetch && cellValue.StartsWith(start))
                {
                    fetch = true;
                }
                else if (fetch)
                {
                    if (cellValue.StartsWith(end))
                    {
                        break;
                    }

                    if (!first && cellValue.Length > 0)
                    {
                        result += "\n";
                    }

                    result += cellValue;
                    first = false;
                }
            }

            return result.Trim();
        }

        protected void SetValueOfProperty<T>(string stringValue, Expression<Func<T, object>> expression, T item)
        {
            MemberExpression expr;

            if (expression.Body is MemberExpression)
            {
                expr = (MemberExpression)expression.Body;
            }
            else
            {
                var op = ((UnaryExpression)expression.Body).Operand;
                expr = (MemberExpression)op;
            }

            var propertyInfo = (PropertyInfo)expr.Member;
            var type = propertyInfo.PropertyType;

            var converter = TypeDescriptor.GetConverter(type);
            if (converter.CanConvertFrom(typeof(string)) && propertyInfo != null)
            {
                if (type == typeof(Decimal))
                {
                    stringValue = RemoveNumberFormatting(stringValue);
                }
                else if (type == typeof(DateTime) && !string.IsNullOrWhiteSpace(DateFormat))
                {
                    propertyInfo.SetValue(item, DateTime.ParseExact(stringValue, DateFormat, null));
                    return;
                }

                propertyInfo.SetValue(item, converter.ConvertFrom(stringValue));
            }
        }

        protected void SetValueOfProperty(string stringValue, string name, object item)
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
            if (converter.CanConvertFrom(typeof(string)) && propertyInfo != null)
            {
                if (type == typeof(Decimal))
                {
                    stringValue = RemoveNumberFormatting(stringValue);
                }
                else if (type == typeof(DateTime) && !string.IsNullOrWhiteSpace(DateFormat))
                {
                    propertyInfo.SetValue(item, DateTime.ParseExact(stringValue, DateFormat, null));
                    return;
                }

                propertyInfo.SetValue(item, converter.ConvertFrom(stringValue));
            }
        }

        private static string RemoveNumberFormatting(string value)
        {
            value = Regex.Replace(value, @"\s+", "");

            var indexOfDot = value.IndexOf('.');
            var indexOfComma = value.IndexOf(',');

            if (indexOfDot > -1 && indexOfComma > -1)
                value = value.Replace(indexOfDot < indexOfComma ? "." : ",", "");

            value = value.Replace(".", ",");
            return value.Length == 0 ? "0" : value;
        }

        protected OrderConfirmation CreateOrderConfirmation(IUnitOfWork uow)
        {
            var vendor = uow.VendorRepository.Get(v => v.Name == VendorName).FirstOrDefault();

            if (vendor == null)
            {
                vendor = new Vendor();
                uow.VendorRepository.Create(vendor);
            }

            var order = new OrderConfirmation
            {
                Vendor = vendor,
                Items = new List<OrderConfirmationItem>()
            };

            return order;
        }

        protected OrderConfirmationItem CreateOrderConfirmationItem(OrderConfirmation order)
        {
            return new OrderConfirmationItem
            {
                Component = new Component
                {
                    Vendor = order.Vendor,
                    VendorId = order.Vendor.Id
                }
            };
        }

        protected static OrderConfirmation ReplaceExistingComponents(OrderConfirmation order, IUnitOfWork uow)
        {
            var addedComponents = new List<Component>();

            foreach (var item in order.Items)
            {
                var currentItem = item;

                var component = addedComponents.FirstOrDefault(c => c.Name == currentItem.Component.Name) ??
                    uow.SingleComponentRepository
                        .Get(c => c.Name == currentItem.Component.Name && c.VendorId == currentItem.Component.VendorId)
                        .FirstOrDefault();

                if (component != null)
                {
                    item.Component = component;
                }
                else
                {
                    uow.SingleComponentRepository.Create(item.Component);
                    addedComponents.Add(item.Component);
                }
            }

            return order;
        }
    }
}