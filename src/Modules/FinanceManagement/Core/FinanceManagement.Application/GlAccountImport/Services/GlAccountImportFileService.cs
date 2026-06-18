using System.Text;
using FinanceManagement.Application.Common.Interfaces.IGlAccountImport;
using FinanceManagement.Application.GlAccountImport.Dto;
using OfficeOpenXml;

namespace FinanceManagement.Application.GlAccountImport.Services
{
    /// <summary>
    /// Excel (EPPlus) + hand-rolled CSV reader/writer for the COA import/export file.
    /// Columns are matched by header name (case-insensitive) so the layout survives column
    /// reordering, and the exported file round-trips back through the parser unchanged (AC5).
    /// </summary>
    public sealed class GlAccountImportFileService : IGlAccountImportFileService
    {
        public const string FormatExcel = "Excel";
        public const string FormatCsv = "Csv";

        public const string ExcelContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        public const string CsvContentType = "text/csv";

        public bool IsSupported(string fileName)
        {
            var ext = Path.GetExtension(fileName)?.ToLowerInvariant();
            return ext is ".xlsx" or ".xls" or ".csv";
        }

        public string ResolveFormat(string fileName)
        {
            var ext = Path.GetExtension(fileName)?.ToLowerInvariant();
            return ext == ".csv" ? FormatCsv : FormatExcel;
        }

        // ── Parse ────────────────────────────────────────────────────────────
        public (IReadOnlyList<GlAccountImportRowDto> Rows, IReadOnlyList<GlAccountImportErrorDto> ParseErrors)
            Parse(Stream stream, string format)
        {
            return format == FormatCsv ? ParseCsv(stream) : ParseExcel(stream);
        }

        private static (IReadOnlyList<GlAccountImportRowDto>, IReadOnlyList<GlAccountImportErrorDto>) ParseExcel(Stream stream)
        {
            var rows = new List<GlAccountImportRowDto>();
            var errors = new List<GlAccountImportErrorDto>();

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
            if (!map.ContainsKey(GlAccountImportColumns.RecordType))
            {
                errors.Add(FileError("Missing required 'RecordType' column. Download a fresh template."));
                return (rows, errors);
            }

            int rowCount = ws.Dimension.Rows;
            for (int r = 2; r <= rowCount; r++)
            {
                var fields = new string[colCount];
                bool allBlank = true;
                for (int c = 1; c <= colCount; c++)
                {
                    var text = ws.Cells[r, c].Text?.Trim() ?? string.Empty;
                    fields[c - 1] = text;
                    if (text.Length > 0) allBlank = false;
                }
                if (allBlank) continue; // ignore fully empty rows

                rows.Add(BuildRow(fields, map, r));
            }

            return (rows, errors);
        }

        private static (IReadOnlyList<GlAccountImportRowDto>, IReadOnlyList<GlAccountImportErrorDto>) ParseCsv(Stream stream)
        {
            var rows = new List<GlAccountImportRowDto>();
            var errors = new List<GlAccountImportErrorDto>();

            using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
            var content = reader.ReadToEnd();
            var records = TokenizeCsv(content);
            if (records.Count == 0)
            {
                errors.Add(FileError("The uploaded CSV is empty."));
                return (rows, errors);
            }

            var header = records[0].Select(h => h?.Trim() ?? string.Empty).ToArray();
            var map = BuildHeaderMap(header);
            if (!map.ContainsKey(GlAccountImportColumns.RecordType))
            {
                errors.Add(FileError("Missing required 'RecordType' column. Download a fresh template."));
                return (rows, errors);
            }

            for (int i = 1; i < records.Count; i++)
            {
                var fields = records[i].Select(f => f?.Trim() ?? string.Empty).ToArray();
                if (fields.All(string.IsNullOrEmpty)) continue;
                rows.Add(BuildRow(fields, map, i + 1)); // +1 → 1-based file row (header is row 1)
            }

            return (rows, errors);
        }

        // Maps known column index → source field index, by case-insensitive header match.
        private static Dictionary<int, int> BuildHeaderMap(string[] sourceHeader)
        {
            var knownByName = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < GlAccountImportColumns.Headers.Length; i++)
                knownByName[GlAccountImportColumns.Headers[i]] = i;

            var map = new Dictionary<int, int>();
            for (int src = 0; src < sourceHeader.Length; src++)
            {
                if (knownByName.TryGetValue(sourceHeader[src], out var known) && !map.ContainsKey(known))
                    map[known] = src;
            }
            return map;
        }

        private static GlAccountImportRowDto BuildRow(string[] fields, Dictionary<int, int> map, int rowNumber)
        {
            string? Get(int known)
            {
                if (!map.TryGetValue(known, out var src) || src >= fields.Length) return null;
                var v = fields[src]?.Trim();
                return string.IsNullOrEmpty(v) ? null : v;
            }

            return new GlAccountImportRowDto
            {
                RowNumber = rowNumber,
                RecordType = Get(GlAccountImportColumns.RecordType),
                GroupCode = Get(GlAccountImportColumns.GroupCode),
                GroupName = Get(GlAccountImportColumns.GroupName),
                ParentGroupCode = Get(GlAccountImportColumns.ParentGroupCode),
                AccountType = Get(GlAccountImportColumns.AccountType),
                SortOrder = Get(GlAccountImportColumns.SortOrder),
                AccountCode = Get(GlAccountImportColumns.AccountCode),
                AccountName = Get(GlAccountImportColumns.AccountName),
                Description = Get(GlAccountImportColumns.Description),
                AccountGroupCode = Get(GlAccountImportColumns.AccountGroupCode),
                NormalBalance = Get(GlAccountImportColumns.NormalBalance),
                Currency = Get(GlAccountImportColumns.Currency),
                SubLedgerType = Get(GlAccountImportColumns.SubLedgerType),
                IsCostCentreMandatory = Get(GlAccountImportColumns.IsCostCentreMandatory),
                IsTaxRelevant = Get(GlAccountImportColumns.IsTaxRelevant),
                IsInterCompany = Get(GlAccountImportColumns.IsInterCompany),
                IsReconciliationRequired = Get(GlAccountImportColumns.IsReconciliationRequired)
            };
        }

        private static GlAccountImportErrorDto FileError(string message) =>
            new() { RowNumber = 0, ColumnName = null, ErrorMessage = message };

        // ── Build template / export ──────────────────────────────────────────
        public GlAccountFileResultDto BuildTemplate(string format)
        {
            var rows = new[]
            {
                GlAccountImportColumns.SampleGroupRow,
                GlAccountImportColumns.SampleAccountRow
            };
            var bytes = format == FormatCsv ? WriteCsv(rows) : WriteExcel(rows);
            return new GlAccountFileResultDto
            {
                Content = bytes,
                FileName = format == FormatCsv ? "COA_Import_Template.csv" : "COA_Import_Template.xlsx",
                ContentType = format == FormatCsv ? CsvContentType : ExcelContentType
            };
        }

        public GlAccountFileResultDto BuildExport(IReadOnlyList<GlAccountImportRowDto> rows, string format)
        {
            var matrix = rows.Select(ToRow).ToList();
            var bytes = format == FormatCsv ? WriteCsv(matrix) : WriteExcel(matrix);
            return new GlAccountFileResultDto
            {
                Content = bytes,
                FileName = format == FormatCsv ? "COA_Export.csv" : "COA_Export.xlsx",
                ContentType = format == FormatCsv ? CsvContentType : ExcelContentType
            };
        }

        private static string[] ToRow(GlAccountImportRowDto r)
        {
            var a = new string[GlAccountImportColumns.ColumnCount];
            a[GlAccountImportColumns.RecordType] = r.RecordType ?? string.Empty;
            a[GlAccountImportColumns.GroupCode] = r.GroupCode ?? string.Empty;
            a[GlAccountImportColumns.GroupName] = r.GroupName ?? string.Empty;
            a[GlAccountImportColumns.ParentGroupCode] = r.ParentGroupCode ?? string.Empty;
            a[GlAccountImportColumns.AccountType] = r.AccountType ?? string.Empty;
            a[GlAccountImportColumns.AccountCode] = r.AccountCode ?? string.Empty;
            a[GlAccountImportColumns.AccountName] = r.AccountName ?? string.Empty;
            a[GlAccountImportColumns.Description] = r.Description ?? string.Empty;
            a[GlAccountImportColumns.AccountGroupCode] = r.AccountGroupCode ?? string.Empty;
            a[GlAccountImportColumns.NormalBalance] = r.NormalBalance ?? string.Empty;
            a[GlAccountImportColumns.Currency] = r.Currency ?? string.Empty;
            a[GlAccountImportColumns.SubLedgerType] = r.SubLedgerType ?? string.Empty;
            a[GlAccountImportColumns.SortOrder] = r.SortOrder ?? string.Empty;
            a[GlAccountImportColumns.IsCostCentreMandatory] = r.IsCostCentreMandatory ?? string.Empty;
            a[GlAccountImportColumns.IsTaxRelevant] = r.IsTaxRelevant ?? string.Empty;
            a[GlAccountImportColumns.IsInterCompany] = r.IsInterCompany ?? string.Empty;
            a[GlAccountImportColumns.IsReconciliationRequired] = r.IsReconciliationRequired ?? string.Empty;
            return a;
        }

        private static byte[] WriteExcel(IReadOnlyList<string[]> rows)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage();
            var ws = package.Workbook.Worksheets.Add("COA");

            for (int c = 0; c < GlAccountImportColumns.Headers.Length; c++)
                ws.Cells[1, c + 1].Value = GlAccountImportColumns.Headers[c];
            ws.Cells[1, 1, 1, GlAccountImportColumns.ColumnCount].Style.Font.Bold = true;

            for (int r = 0; r < rows.Count; r++)
                for (int c = 0; c < GlAccountImportColumns.ColumnCount; c++)
                    ws.Cells[r + 2, c + 1].Value = rows[r][c];

            ws.Cells.AutoFitColumns();
            return package.GetAsByteArray();
        }

        private static byte[] WriteCsv(IReadOnlyList<string[]> rows)
        {
            var sb = new StringBuilder();
            sb.AppendLine(string.Join(",", GlAccountImportColumns.Headers.Select(EscapeCsv)));
            foreach (var row in rows)
                sb.AppendLine(string.Join(",", row.Select(EscapeCsv)));
            // UTF-8 BOM so Excel opens the CSV with correct encoding.
            return new UTF8Encoding(true).GetBytes(sb.ToString());
        }

        private static string EscapeCsv(string? value)
        {
            value ??= string.Empty;
            if (value.Contains('"') || value.Contains(',') || value.Contains('\n') || value.Contains('\r'))
                return "\"" + value.Replace("\"", "\"\"") + "\"";
            return value;
        }

        // RFC-4180 tokenizer: handles quoted fields, escaped quotes ("") and embedded newlines.
        private static List<string[]> TokenizeCsv(string content)
        {
            var records = new List<string[]>();
            var fields = new List<string>();
            var field = new StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < content.Length; i++)
            {
                char ch = content[i];
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
                        case '\r': break; // handled with \n
                        case '\n':
                            fields.Add(field.ToString()); field.Clear();
                            records.Add(fields.ToArray()); fields.Clear();
                            break;
                        default: field.Append(ch); break;
                    }
                }
            }

            // trailing field/record without a final newline
            if (field.Length > 0 || fields.Count > 0)
            {
                fields.Add(field.ToString());
                records.Add(fields.ToArray());
            }

            return records;
        }
    }
}
