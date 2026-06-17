using Contracts.Common;
using FinanceManagement.Application.TaxCode.Dto;
using MediatR;

namespace FinanceManagement.Application.TaxCode.Queries.GetGstrSectionMasterAutoComplete
{
    // Section dropdown for the linkage screen, optionally scoped to a Report.
    public sealed record GetGstrSectionMasterAutoCompleteQuery(string Term, int? ReportTypeId)
        : IRequest<ApiResponseDTO<IReadOnlyList<GstrSectionMasterLookupDto>>>;
}
