using FinanceManagement.Application.CoaReport.Dto;

namespace FinanceManagement.Application.Common.Interfaces.ICoaReport
{
    // US-GL02-15 (AC1/AC5) — renders the COA listing rows into an auditor-ready PDF.
    public interface ICoaListingPdfBuilder
    {
        byte[] Build(string? companyName, DateTimeOffset generatedOn, IReadOnlyList<CoaListingItemDto> rows);
    }
}
