using FinanceManagement.Application.GlAccountImport.Dto;

namespace FinanceManagement.Application.Common.Interfaces.IGlAccountImport
{
    /// <summary>
    /// Reads and writes the COA file in either format. Excel uses EPPlus; CSV is hand-rolled
    /// (RFC-4180 quoting) so no extra package is required. Pure I/O — no DB access, no validation
    /// beyond structural parsing.
    /// </summary>
    public interface IGlAccountImportFileService
    {
        /// <summary>true when the file extension is one we accept (.xlsx/.xls/.csv).</summary>
        bool IsSupported(string fileName);

        /// <summary>"Excel" or "Csv" for the given file name.</summary>
        string ResolveFormat(string fileName);

        /// <summary>
        /// Parses an uploaded file into raw rows. <paramref name="parseErrors"/> captures structural
        /// problems (missing header, unknown RecordType, empty file) keyed by row number.
        /// </summary>
        (IReadOnlyList<GlAccountImportRowDto> Rows, IReadOnlyList<GlAccountImportErrorDto> ParseErrors)
            Parse(Stream stream, string format);

        /// <summary>Builds the empty download template (header + two sample rows).</summary>
        GlAccountFileResultDto BuildTemplate(string format);

        /// <summary>Builds a full COA export from the supplied rows. Re-imports cleanly (AC5).</summary>
        GlAccountFileResultDto BuildExport(IReadOnlyList<GlAccountImportRowDto> rows, string format);
    }
}
