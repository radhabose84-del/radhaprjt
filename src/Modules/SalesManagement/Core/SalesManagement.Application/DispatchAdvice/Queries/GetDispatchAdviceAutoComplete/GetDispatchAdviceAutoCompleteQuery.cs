using MediatR;
using SalesManagement.Application.DispatchAdvice.Dto;

namespace SalesManagement.Application.DispatchAdvice.Queries.GetDispatchAdviceAutoComplete
{
    public sealed record GetDispatchAdviceAutoCompleteQuery(string Term, bool ProformaFilter = false)
        : IRequest<IReadOnlyList<DispatchAdviceLookupDto>>;
}
