using FinanceManagement.Application.MiscTypeMaster.Dto;
using MediatR;

namespace FinanceManagement.Application.MiscTypeMaster.Queries.GetMiscTypeMasterAutoComplete
{
    public sealed record GetMiscTypeMasterAutoCompleteQuery(string Term)
        : IRequest<IReadOnlyList<MiscTypeMasterLookupDto>>;
}
