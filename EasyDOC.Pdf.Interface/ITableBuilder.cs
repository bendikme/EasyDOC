using System.Collections.Generic;
using EasyDOC.Model;

namespace EasyDOC.Pdf.Interface
{
    public interface ITableBuilder
    {
        Documentation Documentation { get; set; }
        bool HasMultipleTables { get; }
        IReadOnlyDictionary<string, string> Parameters { get; set; }
        IEnumerable<DocumentTable> GetTables();
    }
}