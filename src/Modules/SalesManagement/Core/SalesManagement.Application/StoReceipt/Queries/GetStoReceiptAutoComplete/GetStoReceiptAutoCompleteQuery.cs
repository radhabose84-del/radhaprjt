using MediatR;
using SalesManagement.Application.StoReceipt.Dto;

namespace SalesManagement.Application.StoReceipt.Queries.GetStoReceiptAutoComplete
{
    public sealed record GetStoReceiptAutoCompleteQuery(string Term)
        : IRequest<IReadOnlyList<StoReceiptLookupDto>>;
}
