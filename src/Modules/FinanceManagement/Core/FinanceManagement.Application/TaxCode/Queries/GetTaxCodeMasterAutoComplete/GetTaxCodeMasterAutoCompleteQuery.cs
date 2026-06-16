using FinanceManagement.Application.TaxCode.Dto;
using MediatR;

namespace FinanceManagement.Application.TaxCode.Queries.GetTaxCodeMasterAutoComplete
{
    public sealed record GetTaxCodeMasterAutoCompleteQuery(string Term, int? CompanyId, string? TaxType)
        : IRequest<IReadOnlyList<TaxCodeMasterLookupDto>>;
}
