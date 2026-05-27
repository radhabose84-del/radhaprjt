using MediatR;
using PurchaseManagement.Application.PurchaseReturn.PurchaseReturn.Dto;

namespace PurchaseManagement.Application.PurchaseReturn.PurchaseReturn.Queries.GetPurchaseReturnAutoComplete;

public sealed record GetPurchaseReturnAutoCompleteQuery(string? Term)
    : IRequest<IReadOnlyList<PurchaseReturnListItemDto>>;
