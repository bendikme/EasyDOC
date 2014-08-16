using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using EasyDOC.DAL.DataAccess;
using EasyDOC.Model;
using iTextSharp.text;

namespace EasyDOC.Pdf.Interface
{
    public class EmployeeConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value != null)
            {
                var str = value.ToString();
                var uow = UnitOfWork.Instance;

                var employees = uow.EmployeeRepository.GetAll();
                var bestMatch = 0.0;
                Employee bestEmployee = null;

                var enumerable = employees.ToList();

                foreach (var employee in enumerable)
                {
                    var match = CompareStrings(str, employee.Name);

                    if (match > bestMatch)
                    {
                        bestMatch = match;
                        bestEmployee = employee;
                    }
                }

                if (bestMatch < 0.3)
                {
                    foreach (var employee in enumerable)
                    {
                        var match = CompareStrings(str, string.Join("", employee.Name.Split(' ').Select(n => n[0])));

                        if (match > bestMatch)
                        {
                            bestMatch = match;
                            bestEmployee = employee;
                        }
                    }
                }

                return bestEmployee;
            }

            return null;
        }

        private static IEnumerable<string> GetLetterPairs(string input)
        {
            if (input.Length > 1 && !string.IsNullOrWhiteSpace(input))
                return Enumerable.Range(0, input.Length - 1).Select(i => input.Substring(i, 2));

            return new List<string>();
        }

        private static HashSet<string> GetWordPairs(string input)
        {
            return new HashSet<string>(Regex.Split(input, "\\s").SelectMany(GetLetterPairs));
        }

        public static double CompareStrings(string s1, string s2)
        {
            var pairs = GetWordPairs(s1.ToUpperInvariant());
            var pairs2 = GetWordPairs(s2.ToUpperInvariant());

            pairs.IntersectWith(GetWordPairs(s2.ToUpperInvariant()));
            return (2.0 * pairs.Count) / (pairs.Count() + pairs2.Count());
        }
    }
}