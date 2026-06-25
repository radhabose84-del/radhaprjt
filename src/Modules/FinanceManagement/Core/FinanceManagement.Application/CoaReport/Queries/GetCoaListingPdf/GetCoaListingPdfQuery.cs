using FinanceManagement.Application.CoaReport.Dto;
using MediatR;

namespace FinanceManagement.Application.CoaReport.Queries.GetCoaListingPdf
{
    // US-GL02-15 (AC1/AC5) — render the COA listing to an auditor-ready PDF download.
    public class GetCoaListingPdfQuery : IRequest<ReportFileResultDto>
    {
        public int? AccountTypeId { get; set; }
        public int? AccountGroupId { get; set; }
        public bool ActiveOnly { get; set; }
        public string? SearchTerm { get; set; }
    }
}
