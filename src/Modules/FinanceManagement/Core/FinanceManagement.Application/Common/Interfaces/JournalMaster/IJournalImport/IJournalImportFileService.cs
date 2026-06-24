using FinanceManagement.Application.JournalMaster.Dto;

namespace FinanceManagement.Application.Common.Interfaces.JournalMaster.IJournalImport
{
    // US-GL01-17 — reads an uploaded journal-import workbook/CSV into typed rows (header-name matched,
    // case-insensitive) and produces a downloadable template.
    public interface IJournalImportFileService
    {
        bool IsSupported(string fileName);

        (IReadOnlyList<JournalImportRowInputDto> Rows, IReadOnlyList<JournalImportErrorDto> ParseErrors)
            Parse(Stream stream, string fileName);

        JournalImportFileDto BuildTemplate();
    }
}
