using System.Collections.Generic;
using System.Linq;
using LinqToExcel;

namespace EasyDOC.Pdf.Interface
{
    /// <summary>
    /// Strategy class that strips a spantech excel file for everything but the
    /// data needed for part details extraction. This strip process cuts some
    /// megabytes off the total file size.
    /// </summary>
    public class StripSpantechExcelFile
    {
        private readonly string _filename;

        public StripSpantechExcelFile(string filename)
        {
            _filename = filename;
        }

        public List<List<string>> Parse()
        {
            var book = new ExcelQueryFactory(_filename);
            var data = new List<List<string>>();

            var sheets = book.GetWorksheetNames().Where(name => name.EndsWith("-BOM") || name.Equals("Combine Sheet"));

            foreach (var rows in sheets.Select(book.WorksheetNoHeader))
            {
                foreach (var row in rows.Skip(2))
                {
                    var currentLine = new List<string>();
                    data.Add(currentLine);

                    var empty = true;
                    for (int i = 0; i < 10; ++i)
                    {
                        var cell = row[i].Value.ToString();
                        currentLine.Add(cell);
                        empty &= cell.Length == 0;
                    }

                    if (empty) break;
                }
            }

            return data;
        }
    }
}