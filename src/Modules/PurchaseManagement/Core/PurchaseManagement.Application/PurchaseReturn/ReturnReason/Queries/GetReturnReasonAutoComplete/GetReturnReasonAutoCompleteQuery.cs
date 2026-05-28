using MediatR;
using PurchaseManagement.Application.PurchaseReturn.ReturnReason.Dto;

namespace PurchaseManagement.Application.PurchaseReturn.ReturnReason.Queries.GetReturnReasonAutoComplete;

public sealed record GetReturnReasonAutoCompleteQuery(string? Term)
    : IRequest<IReadOnlyList<ReturnReasonLookupDto>>;
