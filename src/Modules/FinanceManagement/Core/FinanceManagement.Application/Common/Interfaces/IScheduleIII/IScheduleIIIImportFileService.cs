using FinanceManagement.Application.ScheduleIII.Dto;

namespace FinanceManagement.Application.Common.Interfaces.IScheduleIII
{
    // Reads an uploaded Schedule III section/line-item workbook/CSV into typed rows (header-name matched,
    // case-insensitive) and produces a downloadable template.
    public interface IScheduleIIIImportFileService
    {
        bool IsSupported(string fileName);

        (IReadOnlyList<ScheduleIIIImportRowInputDto> Rows, IReadOnlyList<ScheduleIIIImportErrorDto> ParseErrors)
            Parse(Stream stream, string fileName);

        ScheduleIIIImportFileDto BuildTemplate();
    }
}
