using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Main.HelpServices
{
    public static class CsvHelper
    {
        public static void ExportDataGridViewToCsv(DataGridView dgv, string path, bool includeHeaders = true)
        {
            using var writer = new StreamWriter(path, false, new UTF8Encoding(false));

            var cols = dgv.Columns.Cast<DataGridViewColumn>()
                        .Where(c => c.Visible)                // respect visibility
                        .OrderBy(c => c.DisplayIndex)         // respect order
                        .ToList();

            if (includeHeaders)
                WriteCsvRow(writer, cols.Select(c => c.HeaderText));

            foreach (DataGridViewRow row in dgv.Rows)
            {
                if (row.IsNewRow) continue;
                var cells = cols.Select(c => row.Cells[c.Index].Value?.ToString() ?? "");
                WriteCsvRow(writer, cells);
            }
        }


        private static void WriteCsvRow(TextWriter writer, IEnumerable<string> fields)
        {
            static string Escape(string? f)
            {
                if (string.IsNullOrEmpty(f)) return "";
                var needsQuote = f.Contains(',') || f.Contains('"') || f.Contains('\n') || f.Contains('\r');
                var cleaned = f.Replace("\"", "\"\"");
                return needsQuote ? $"\"{cleaned}\"" : cleaned;
            }

            writer.WriteLine(string.Join(",", fields.Select(Escape)));
        }

    }
}
