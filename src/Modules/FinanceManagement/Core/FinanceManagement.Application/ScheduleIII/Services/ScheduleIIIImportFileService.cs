using System.Text;
using FinanceManagement.Application.Common.Interfaces.IScheduleIII;
using FinanceManagement.Application.ScheduleIII.Dto;
using OfficeOpenXml;

namespace FinanceManagement.Application.ScheduleIII.Services
{
    // Excel (EPPlus) + CSV reader for the Schedule III section/line-item import. Columns are matched by header
    // name (case-insensitive) so the layout survives reordering. Unparseable cells become parse errors.
    public sealed class ScheduleIIIImportFileService : IScheduleIIIImportFileService
    {
        private const string ExcelContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

        private static readonly string[] Headers =
        {
            "RowNo", "SectionName", "StatementType", "Nature", "LineCode", "LineName", "NoteReference", "IsSplitLine"
        };

        private static readonly string[] RequiredHeaders =
        {
            "SectionName", "StatementType", "Nature", "LineCode", "LineName"
        };

        public bool IsSupported(string fileName)
        {
            var ext = Path.GetExtension(fileName)?.ToLowerInvariant();
            return ext is ".xlsx" or ".xls" or ".csv";
        }

        public (IReadOnlyList<ScheduleIIIImportRowInputDto> Rows, IReadOnlyList<ScheduleIIIImportErrorDto> ParseErrors)
            Parse(Stream stream, string fileName)
        {
            var ext = Path.GetExtension(fileName)?.ToLowerInvariant();
            return ext == ".csv" ? ParseCsv(stream) : ParseExcel(stream);
        }

        private static (IReadOnlyList<ScheduleIIIImportRowInputDto>, IReadOnlyList<ScheduleIIIImportErrorDto>) ParseExcel(Stream stream)
        {
            var rows = new List<ScheduleIIIImportRowInputDto>();
            var errors = new List<ScheduleIIIImportErrorDto>();

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage(stream);
            var ws = package.Workbook.Worksheets.FirstOrDefault();
            if (ws?.Dimension == null)
            {
                errors.Add(FileError("The uploaded workbook is empty."));
                return (rows, errors);
            }

            int colCount = ws.Dimension.Columns;
            var header = new string[colCount];
            for (int c = 1; c <= colCount; c++)
                header[c - 1] = ws.Cells[1, c].Text?.Trim() ?? string.Empty;

            var map = BuildHeaderMap(header);
            var missing = MissingRequired(map);
            if (missing != null)
            {
                errors.Add(FileError(missing));
                return (rows, errors);
            }

            int rowCount = ws.Dimension.Rows;
            for (int r = 2; r <= rowCount; r++)
            {
                var fields = new string[colCount];
                var allBlank = true;
                for (int c = 1; c <= colCount; c++)
                {
                    var text = ws.Cells[r, c].Text?.Trim() ?? string.Empty;
                    fields[c - 1] = text;
                    if (text.Length > 0) allBlank = false;
                }
                if (allBlank) continue;

                rows.Add(BuildRow(fields, map, r));
            }

            return (rows, errors);
        }

        private static (IReadOnlyList<ScheduleIIIImportRowInputDto>, IReadOnlyList<ScheduleIIIImportErrorDto>) ParseCsv(Stream stream)
        {
            var rows = new List<ScheduleIIIImportRowInputDto>();
            var errors = new List<ScheduleIIIImportErrorDto>();

            using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
            var records = TokenizeCsv(reader.ReadToEnd());
            if (records.Count == 0)
            {
                errors.Add(FileError("The uploaded CSV is empty."));
                return (rows, errors);
            }

            var header = records[0].Select(h => h?.Trim() ?? string.Empty).ToArray();
            var map = BuildHeaderMap(header);
            var missing = MissingRequired(map);
            if (missing != null)
            {
                errors.Add(FileError(missing));
                return (rows, errors);
            }

            for (int i = 1; i < records.Count; i++)
            {
                var fields = records[i].Select(f => f?.Trim() ?? string.Empty).ToArray();
                if (fields.All(string.IsNullOrEmpty)) continue;
                rows.Add(BuildRow(fields, map, i + 1));
            }

            return (rows, errors);
        }

        private static Dictionary<string, int> BuildHeaderMap(string[] sourceHeader)
        {
            var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < sourceHeader.Length; i++)
            {
                var name = sourceHeader[i];
                if (!string.IsNullOrEmpty(name) && Headers.Contains(name, StringComparer.OrdinalIgnoreCase) && !map.ContainsKey(name))
                    map[name] = i;
            }
            return map;
        }

        private static string? MissingRequired(Dictionary<string, int> map)
        {
            var missing = RequiredHeaders.Where(h => !map.ContainsKey(h)).ToList();
            return missing.Count == 0 ? null
                : $"Missing required column(s): {string.Join(", ", missing)}. Download a fresh template.";
        }

        private static ScheduleIIIImportRowInputDto BuildRow(string[] fields, Dictionary<string, int> map, int fileRow)
        {
            string? Get(string col)
            {
                if (!map.TryGetValue(col, out var src) || src >= fields.Length) return null;
                var v = fields[src]?.Trim();
                return string.IsNullOrEmpty(v) ? null : v;
            }

            bool Bool(string col)
            {
                var v = Get(col)?.ToLowerInvariant();
                return v is "1" or "true" or "yes" or "y";
            }

            var rowNo = Get("RowNo") is { } rn && int.TryParse(rn, out var parsed) ? parsed : fileRow;

            return new ScheduleIIIImportRowInputDto
            {
                RowNo = rowNo,
                SectionName = Get("SectionName"),
                StatementType = Get("StatementType"),
                Nature = Get("Nature"),
                LineCode = Get("LineCode"),
                LineName = Get("LineName"),
                NoteReference = Get("NoteReference"),
                IsSplitLine = Bool("IsSplitLine")
            };
        }

        private static ScheduleIIIImportErrorDto FileError(string message) =>
            new() { RowNo = 0, ColumnName = null, Message = message };

        public ScheduleIIIImportFileDto BuildTemplate()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage();
            var ws = package.Workbook.Worksheets.Add("ScheduleIIIImport");

            for (int c = 0; c < Headers.Length; c++)
                ws.Cells[1, c + 1].Value = Headers[c];
            ws.Cells[1, 1, 1, Headers.Length].Style.Font.Bold = true;

            // sample = two line items under one section + one under another
            object?[,] sample =
            {
                { 1, "Revenue from Operations", "PL", "Income",  "REV01", "Sale of products",   "Note 20", "false" },
                { 2, "Revenue from Operations", "PL", "Income",  "REV02", "Other operating income", "Note 21", "false" },
                { 3, "Other Expenses",          "PL", "Expense", "EXP01", "Power & fuel",        "Note 25", "false" }
            };
            for (int r = 0; r < sample.GetLength(0); r++)
                for (int c = 0; c < sample.GetLength(1); c++)
                    ws.Cells[r + 2, c + 1].Value = sample[r, c];

            ws.Cells.AutoFitColumns();

            return new ScheduleIIIImportFileDto
            {
                Content = package.GetAsByteArray(),
                FileName = "ScheduleIII_Import_Template.xlsx",
                ContentType = ExcelContentType
            };
        }

        // RFC-4180 tokenizer: quoted fields, escaped quotes, embedded newlines.
        private static List<string[]> TokenizeCsv(string content)
        {
            var records = new List<string[]>();
            var fields = new List<string>();
            var field = new StringBuilder();
            var inQuotes = false;

            for (int i = 0; i < content.Length; i++)
            {
                var ch = content[i];
                if (inQuotes)
                {
                    if (ch == '"')
                    {
                        if (i + 1 < content.Length && content[i + 1] == '"') { field.Append('"'); i++; }
                        else inQuotes = false;
                    }
                    else field.Append(ch);
                }
                else
                {
                    switch (ch)
                    {
                        case '"': inQuotes = true; break;
                        case ',': fields.Add(field.ToString()); field.Clear(); break;
                        case '\r': break;
                        case '\n': fields.Add(field.ToString()); field.Clear(); records.Add(fields.ToArray()); fields.Clear(); break;
                        default: field.Append(ch); break;
                    }
                }
            }

            if (field.Length > 0 || fields.Count > 0)
            {
                fields.Add(field.ToString());
                records.Add(fields.ToArray());
            }

            return records;
        }
    }
}
