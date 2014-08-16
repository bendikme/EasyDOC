using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using EasyDOC.DAL.DataAccess;
using EasyDOC.Model;
using EasyDOC.Pdf.Interface;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace EasyDOC.Pdf.ManualTableGenerators
{
    class SpantechDataTableBuilder : ITableBuilder
    {
        public string RootDirectory { get; set; }
        public Documentation Documentation { get; set; }
        public string PdfConverterRules { get; set; }
        public string Titles { get; set; }
        public string Widths { get; set; }

        private readonly List<SpantechPart> _parts;
        private readonly List<SpantechPart> _filteredParts;

        public SpantechDataTableBuilder()
        {
            _parts = new List<SpantechPart>();
            _filteredParts = new List<SpantechPart>();
        }

        private void ExtractDataFromCsv(string file)
        {
            var fullName = string.Format(@"{0}\{1}", RootDirectory, file);

            var strategy = new StripSpantechExcelFile(fullName);
            var lines = strategy.Parse();

            string projectNumber = "";
            bool spantechMode = !string.IsNullOrWhiteSpace(lines[0][0]);
            int offset = spantechMode ? 1 : 0;

            var lastCode = 0;
            var stack = new Stack<SpantechPart>();

            foreach (var line in lines)
            {
                if (!spantechMode && line.Count() > 1 && string.IsNullOrWhiteSpace(line[0]) && !string.IsNullOrWhiteSpace(line[1]))
                {
                    projectNumber = line[1];
                }
                else if (!string.IsNullOrWhiteSpace(line[4 + offset]) && line[1] != "Code")
                {
                    try
                    {
                        var part = new SpantechPart
                        {
                            ProjectNumber = (spantechMode ? line[0] : projectNumber).Trim(),
                            Code = int.Parse(line[0 + offset]),
                            PartNumber = line[1 + offset],
                            PartDesc = line[2 + offset],
                            UnitOfMeasurement = line[3 + offset],
                            TotalAmount = Double.Parse(line[4 + offset]),
                            Info1 = line[7 + offset],
                            Info2 = line[8 + offset]
                        };

                        if (part.Code <= lastCode)
                        {
                            while (stack.Count > 0 && stack.Peek().Code >= part.Code)
                            {
                                stack.Pop();
                            }
                        }

                        stack.Push(part);
                        lastCode = part.Code;

                        if (spantechMode && stack.All(p => p.UnitOfMeasurement == "EA"))
                        {
                            part.TotalAmount = stack.Aggregate<SpantechPart, double>(1, (current, p) => current * p.TotalAmount);
                        }

                        _parts.Add(part);
                    }
                    catch (Exception e)
                    {
                        Trace.WriteLine(e.Message);
                    }
                }
            }

            // Load all filtered data. Use this to filter the data loaded unfiltered data in-place.
            var filter = string.Format(@"{0}\{1}", RootDirectory, PdfConverterRules);
            var filterValues = System.IO.File.ReadAllLines(filter, Encoding.Default).Select(line => line.Split(';')).ToArray();

            _filteredParts.AddRange(filterValues.Select(line => new SpantechPart
            {
                PartDesc = line[0],
                PartDescNo = line[1],
                PartDescSe = line[2],
                PartReference = line[3],
                Drawing = line[4],
                Unit = line[5],
                SpareAmount = line[6]
            }));
        }

        private void FilterSpantechData()
        {
            var lockTabCorrection = _parts
                .Where(p => p.UnitOfMeasurement == "R25" || p.UnitOfMeasurement == "R50" || p.PartNumber.StartsWith("DS0140"))
                .GroupBy(p => p.ProjectNumber);

            foreach (var project in lockTabCorrection)
            {
                var links = project.FirstOrDefault(p => p.UnitOfMeasurement == "R25" || p.UnitOfMeasurement == "R50");
                if (links != null)
                {
                    var lockTabs = project.Where(p => p.PartNumber.StartsWith("DS0140"));
                    foreach (var lockTab in lockTabs)
                    {
                        lockTab.TotalAmount = links.TotalAmount * 2;
                    }
                }
            }

            // Remove all parts not found in the filtered list
            foreach (var part in _parts.Where(part => !string.IsNullOrWhiteSpace(part.PartDesc)))
            {
                var filteredPart = _filteredParts.FirstOrDefault(fp => fp.PartDesc.ToLower() == part.PartDesc.ToLower());
                if (filteredPart != null)
                {
                    part.IsIncludedInList = true;
                    part.PartDescNo = filteredPart.PartDescNo;
                    part.PartDescSe = filteredPart.PartDescSe;
                    part.PartReference = filteredPart.PartReference;
                    part.Drawing = filteredPart.Drawing;
                    part.Unit = filteredPart.Unit;
                    part.SpareAmount = filteredPart.SpareAmount;

                    part.ConveyorWidth = "-";

                    if (part.Drawing == "*trnr*")
                    {
                        part.Drawing = (part.ProjectNumber.Split('-')[0]).Trim() + ".pdf";
                    }

                    if (part.UnitOfMeasurement == "R25" || part.UnitOfMeasurement == "R50")
                    {
                        part.TotalAmount *= (part.UnitOfMeasurement == "R25" ? 25.4 : 50.8) / 1000;
                        part.ConveyorWidth = (GetConveyorWidth(part) * 25.4 / 100).ToString("#.#");
                    }
                    else if (part.UnitOfMeasurement == "FT")
                    {
                        part.TotalAmount *= 0.3048;
                    }
                    else if (part.UnitOfMeasurement == "IN")
                    {
                        part.TotalAmount *= 0.0254;
                    }
                    else if (part.PartDesc.Substring(0, 7) == "BEARIN")
                    {
                        part.TotalAmount = 4;
                    }
                }
            }

            _parts.RemoveAll(part => !part.IsIncludedInList);

            foreach (var project in _parts
                .GroupBy(part => part.ProjectNumber))
            {
                foreach (var group in project
                    .GroupBy(part => part.PartNumber)
                    .Where(g => g.Count() > 1))
                {
                    group.ElementAt(0).TotalAmount = group.Sum(g => g.TotalAmount);

                    foreach (var p in group.Skip(1))
                    {
                        p.IsIncludedInList = false;
                    }
                }
            }

            _parts.RemoveAll(part => !part.IsIncludedInList);
        }

        private static int GetConveyorWidth(SpantechPart part)
        {
            int number;
            if (!Int32.TryParse(part.PartNumber.Substring(4, 4), out number))
                Int32.TryParse(part.PartNumber.Substring(3, 4), out number);
            return number;
        }

        public IEnumerable<DocumentTable> GetTables()
        {
            var table = new PdfPTable(9)
            {
                WidthPercentage = 100
            };

            table.SetWidths(Widths.Split(',').Select(int.Parse).ToArray());
            table.DefaultCell.BackgroundColor = new GrayColor(0.9f);

            var titles = Titles.Split(',');

            table.AddCell(new Phrase(titles[0], PdfFonts.Bold));
            table.AddCell(new Phrase(titles[1], PdfFonts.Bold));
            table.AddCell(new Phrase(titles[2], PdfFonts.Bold));
            table.AddCell(new Phrase(titles[3], PdfFonts.Bold));
            table.AddCell(new Phrase(titles[4], PdfFonts.Bold));
            table.AddCell(new Phrase(titles[5], PdfFonts.Bold));
            table.AddCell(new Phrase(titles[6], PdfFonts.Bold));
            table.AddCell(new Phrase(titles[7], PdfFonts.Bold));
            table.AddCell(new Phrase(titles[8], PdfFonts.Bold));

            table.HeaderRows = 1;

            table.DefaultCell.BackgroundColor = BaseColor.WHITE;

            foreach (var file in Documentation.Project.Files
                .Where(f => f.Role == FileRole.Spantech && f.IncludeInManual)
                .Select(f => f.File))
            {
                ExtractDataFromCsv(file.GetPath());
            }

            FilterSpantechData();

            foreach (var row in _parts
                .OrderBy(p => p.ProjectNumber)
                .ThenBy(p => p.Drawing)
                .ThenBy(p => p.PartNumber))
            {
                table.AddCell(new Phrase(row.ProjectNumber, PdfFonts.Normal));
                table.AddCell(new Phrase(row.PartNumber, PdfFonts.Normal));
                table.AddCell(new Phrase(row.PartDesc, PdfFonts.Normal));
                table.AddCell(new Phrase(row.PartDescNo, PdfFonts.Normal));

                var r = row;
                var manual = new UnitOfWork().FileRepository.Get(m => m.Name == r.Drawing.Replace(".pdf", "")).SingleOrDefault();

                if (manual != null)
                {
                    table.AddCell(new Anchor(new Phrase(row.Drawing, PdfFonts.Link))
                    {
                        Reference = manual.GetPath()
                    });
                }
                else
                {
                    table.AddCell(new Phrase(row.Drawing, PdfFonts.Normal));
                }

                table.AddCell(new Phrase(row.Unit, PdfFonts.Normal));
                table.AddCell(new Phrase(row.TotalAmount.ToString("0.#"), PdfFonts.Normal));
                table.AddCell(new Phrase(row.SpareAmount, PdfFonts.Normal));
                table.AddCell(new Phrase(row.ConveyorWidth, PdfFonts.Normal));
            }

            return new List<DocumentTable> { new DocumentTable("", table) };
        }

        public bool HasMultipleTables
        {
            get { return false; }
        }

        public IReadOnlyDictionary<string, string> Parameters { get; set; }
    }
}