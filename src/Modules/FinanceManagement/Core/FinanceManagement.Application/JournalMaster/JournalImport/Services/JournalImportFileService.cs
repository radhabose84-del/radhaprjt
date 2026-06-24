using System.Globalization;
using System.Text;
using FinanceManagement.Application.Common.Interfaces.JournalMaster.IJournalImport;
using FinanceManagement.Application.JournalMaster.Dto;
using OfficeOpenXml;

namespace FinanceManagement.Application.JournalMaster.JournalImport.Services
{
    // Excel (EPPlus) + CSV reader for the journal-import file. Columns are matched by header name
    // (case-insensitive) so the layout survives reordering. Unparseable cells become row-level parse errors.
    public sealed class JournalImportFileService : IJournalImportFileService
    {
        private const string ExcelContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

        private static readonly string[] Headers =
        {
            "RowNo", "GroupNo", "VoucherTypeId", "VoucherDate", "Narration", "GlAccountId",
            "DrAmount", "CrAmount", "CurrencyId", "CostCentreId", "ProfitCentreId", "LineNarration", "ReferenceDocNo"
        };

        // Headers that must be present for the file to be importable.
        private static readonly string[] RequiredHeaders =
        {
            "GroupNo", "VoucherTypeId", "VoucherDate", "GlAccountId", "CurrencyId", "DrAmount", "CrAmount"
        };

        private static readonly string[] DateFormats =
        {
            "yyyy-MM-dd", "dd-MM-yyyy", "dd/MM/yyyy", "MM/dd/yyyy", "yyyy/MM/dd"
        };

        public bool IsSupported(string fileName)
        {
            var ext = Path.GetExtension(fileName)?.ToLowerInvariant();
            return ext is ".xlsx" or ".xls" or ".csv";
        }

        public (IReadOnlyList<JournalImportRowInputDto> Rows, IReadOnlyList<JournalImportErrorDto> ParseErrors)
            Parse(Stream stream, string fileName)
        {
            var ext = Path.GetExtension(fileName)?.ToLowerInvariant();
            return ext == ".csv" ? ParseCsv(stream) : ParseExcel(stream);
        }

        // ── Excel ────────────────────────────────────────────────────────────
        private static (IReadOnlyList<JournalImportRowInputDto>, IReadOnlyList<JournalImportErrorDto>) ParseExcel(Stream stream)
        {
            var rows = new List<JournalImportRowInputDto>();
            var errors = new List<JournalImportErrorDto>();

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

                rows.Add(BuildRow(fields, map, r, errors));
            }

            return (rows, errors);
        }

        // ── CSV ──────────────────────────────────────────────────────────────
        private static (IReadOnlyList<JournalImportRowInputDto>, IReadOnlyList<JournalImportErrorDto>) ParseCsv(Stream stream)
        {
            var rows = new List<JournalImportRowInputDto>();
            var errors = new List<JournalImportErrorDto>();

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
                rows.Add(BuildRow(fields, map, i + 1, errors));   // +1 → 1-based file row (header is row 1)
            }

            return (rows, errors);
        }

        // header name → source column index (case-insensitive).
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

        private static JournalImportRowInputDto BuildRow(string[] fields, Dictionary<string, int> map, int fileRow, List<JournalImportErrorDto> errors)
        {
            string? Get(string col)
            {
                if (!map.TryGetValue(col, out var src) || src >= fields.Length) return null;
                var v = fields[src]?.Trim();
                return string.IsNullOrEmpty(v) ? null : v;
            }

            int ReqInt(string col)
            {
                var v = Get(col);
                if (v == null) return 0;
                if (int.TryParse(v, NumberStyles.Integer, CultureInfo.InvariantCulture, out var n)) return n;
                errors.Add(new JournalImportErrorDto { RowNo = fileRow, ColumnName = col, Message = $"'{v}' is not a valid number." });
                return 0;
            }

            int? OptInt(string col)
            {
                var v = Get(col);
                if (v == null) return null;
                if (int.TryParse(v, NumberStyles.Integer, CultureInfo.InvariantCulture, out var n)) return n;
                errors.Add(new JournalImportErrorDto { RowNo = fileRow, ColumnName = col, Message = $"'{v}' is not a valid number." });
                return null;
            }

            decimal Dec(string col)
            {
                var v = Get(col);
                if (v == null) return 0m;
                if (decimal.TryParse(v, NumberStyles.Number, CultureInfo.InvariantCulture, out var d)) return d;
                errors.Add(new JournalImportErrorDto { RowNo = fileRow, ColumnName = col, Message = $"'{v}' is not a valid amount." });
                return 0m;
            }

            DateOnly Date(string col)
            {
                var v = Get(col);
                if (v == null)
                {
                    errors.Add(new JournalImportErrorDto { RowNo = fileRow, ColumnName = col, Message = "Voucher date is required." });
                    return default;
                }
                if (DateOnly.TryParseExact(v, DateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var d)) return d;
                if (DateOnly.TryParse(v, CultureInfo.InvariantCulture, DateTimeStyles.None, out d)) return d;
                if (DateTime.TryParse(v, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt)) return DateOnly.FromDateTime(dt);
                errors.Add(new JournalImportErrorDto { RowNo = fileRow, ColumnName = col, Message = $"'{v}' is not a valid date (use yyyy-MM-dd)." });
                return default;
            }

            var rowNo = Get("RowNo") is { } rn && int.TryParse(rn, out var parsedRowNo) ? parsedRowNo : fileRow;

            return new JournalImportRowInputDto
            {
                RowNo = rowNo,
                GroupNo = ReqInt("GroupNo"),
                VoucherTypeId = ReqInt("VoucherTypeId"),
                VoucherDate = Date("VoucherDate"),
                Narration = Get("Narration"),
                GlAccountId = ReqInt("GlAccountId"),
                DrAmount = Dec("DrAmount"),
                CrAmount = Dec("CrAmount"),
                CurrencyId = ReqInt("CurrencyId"),
                CostCentreId = OptInt("CostCentreId"),
                ProfitCentreId = OptInt("ProfitCentreId"),
                LineNarration = Get("LineNarration"),
                ReferenceDocNo = Get("ReferenceDocNo")
            };
        }

        private static JournalImportErrorDto FileError(string message) =>
            new() { RowNo = 0, ColumnName = null, Message = message };

        // ── Template ───────────────────────────────────────────────────────────
        public JournalImportFileDto BuildTemplate()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage();
            var ws = package.Workbook.Worksheets.Add("JournalImport");

            for (int c = 0; c < Headers.Length; c++)
                ws.Cells[1, c + 1].Value = Headers[c];
            ws.Cells[1, 1, 1, Headers.Length].Style.Font.Bold = true;

            // two sample rows = one balanced voucher (group 1)
            object?[,] sample =
            {
                { 1, 1, 1, "2026-06-10", "Adj - depreciation", 1, 100000, 0, 1, 2, 1, "Dep expense", "ADJ/01" },
                { 2, 1, 1, "2026-06-10", "Adj - depreciation", 2, 0, 100000, 1, null, 2, "Accum dep", "ADJ/01" }
            };
            for (int r = 0; r < sample.GetLength(0); r++)
                for (int c = 0; c < sample.GetLength(1); c++)
                    ws.Cells[r + 2, c + 1].Value = sample[r, c];

            ws.Cells.AutoFitColumns();

            return new JournalImportFileDto
            {
                Content = package.GetAsByteArray(),
                FileName = "Journal_Import_Template.xlsx",
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
