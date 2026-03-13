using FinanceManagement.Application.EWaybillHeader.Dto;
using MediatR;

namespace FinanceManagement.Application.EWaybillHeader.Queries.GetEWaybillHeaderAutoComplete
{
    public sealed record GetEWaybillHeaderAutoCompleteQuery(string? Term)
        : IRequest<IReadOnlyList<EWaybillHeaderLookupDto>>;
}
