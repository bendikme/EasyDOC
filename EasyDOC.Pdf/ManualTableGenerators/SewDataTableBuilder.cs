using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using EasyDOC.Model;
using EasyDOC.Pdf.Interface;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.Ajax.Utilities;

namespace EasyDOC.Pdf.ManualTableGenerators
{
    class SewDataTableBuilder : ITableBuilder
    {
        public string RootDirectory { get; set; }
        public Documentation Documentation { get; set; }
        public string PdfConverterPath { get; set; }
        public string PdfConverterRules { get; set; }
        public string Titles { get; set; }
        public string Widths { get; set; }

        private IEnumerable<IEnumerable<Tuple<string, string>>> ExtractDataFromPdf(string file)
        {

            var arguments = string.Format(@"-R""{0}\{1}"" -F""{0}\{2}"" -O""{0}\{2}.csv""", RootDirectory, PdfConverterRules, file);
            int exitCode;

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
                proc.WaitForExit(5000);
                exitCode = proc.ExitCode;
            }

            if (exitCode != 0)
            {
                return null;
            }

            var list = new List<List<Tuple<string, string>>>();
            var rawLines = System.IO.File.ReadAllLines(string.Format(@"{0}\{1}.csv", RootDirectory, file), Encoding.Default);

            var splitLines = rawLines.Select(line => line.Split(new[] { @""",""" }, StringSplitOptions.RemoveEmptyEntries)).ToArray();

            var currentLine = 0;
            var fetching = false;
            var fetchFrom = 0;
            var fetchAreas = new List<Tuple<int, int>>();

            // Find areas to include
            while (currentLine < splitLines.Count())
            {
                var firstColumn = splitLines[currentLine][0].Trim();

                if (!fetching)
                {
                    if (Regex.IsMatch(firstColumn, @"(\d)+ STK"))
                    {
                        fetching = true;
                        fetchFrom = currentLine;
                    }
                }
                else if (firstColumn == "\"Varekode" || firstColumn == "\"Nettovekt [kg]")
                {
                    fetching = false;
                    fetchAreas.Add(new Tuple<int, int>(fetchFrom, currentLine));
                }

                ++currentLine;
            }

            foreach (var area in fetchAreas)
            {
                var areaList = new List<Tuple<string, string>>();
                var lastKey = "";
                var lastValue = "";

                for (int i = area.Item1; i < area.Item2; ++i)
                {
                    var key = splitLines[i][0].Replace("\"", "");
                    var value = splitLines[i][1].Replace("\"", "");

                    if (key.Any() && lastKey.Any())
                    {
                        areaList.Add(new Tuple<string, string>(lastKey, lastValue.StartsWith(": ") ? lastValue.Substring(2) : ""));
                        lastValue = "";
                    }

                    if (key.Any())
                    {
                        lastKey = key;
                    }

                    lastValue += value;
                }

                if (lastKey.Any())
                {
                    areaList.Add(new Tuple<string, string>(lastKey, lastValue.StartsWith(": ") ? lastValue.Substring(2) : ""));
                }

                var count = areaList[0].Item1;
                areaList[0] = new Tuple<string, string>(count.Substring(0, count.IndexOf("STK", StringComparison.Ordinal) + 3), areaList[0].Item2);

                list.Add(areaList);
            }

            return list;
        }

        public bool HasMultipleTables
        {
            get { return true; }
        }

        public IReadOnlyDictionary<string, string> Parameters { get; set; }

        public IEnumerable<DocumentTable> GetTables()
        {
            var tables = new List<DocumentTable>();

            foreach (var file in Documentation.Project.Files
                .Where(f => f.Role == FileRole.SEW && f.IncludeInManual)
                .Select(f => f.File))
            {
                var resultSet = ExtractDataFromPdf(file.GetPath());

                if (resultSet != null)
                {
                    foreach (var order in resultSet)
                    {
                        var table = new PdfPTable(2)
                        {
                            WidthPercentage = 100
                        };

                        var title = order.ElementAt(1).Item1;

                        foreach (var line in order)
                        {
                            if (line.Item1 == "Skilttekst")
                            {
                                title = line.Item2;
                            }
                            table.AddCell(new Phrase(line.Item1, PdfFonts.Normal));
                            table.AddCell(new Phrase(line.Item2, PdfFonts.Normal));
                        }

                        tables.Add(new DocumentTable(title, table));
                    }
                }
                else
                {
                    // TODO: add to error list
                }

            }

            tables.Sort((t1, t2) => String.Compare(t1.Title, t2.Title, StringComparison.Ordinal));

            var currentMotor = 1;
            var filteredElements = new List<DocumentTable>();
            foreach (var table in tables)
            {
                var motorName = "name" + currentMotor;
                var exclude = "exclude" + currentMotor;
                if (Parameters.ContainsKey(motorName))
                {
                    table.Title = Parameters[motorName];
                }

                if (!Parameters.ContainsKey(exclude) || !Boolean.Parse(Parameters[exclude]))
                {
                    filteredElements.Add(table);
                }
                ++currentMotor;
            }

            return filteredElements;
        }
    }
}