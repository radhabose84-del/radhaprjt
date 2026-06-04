using MediatR;
using PurchaseManagement.Application.PurchaseReturn.PurchaseReturn.Dto;

namespace PurchaseManagement.Application.PurchaseReturn.PurchaseReturn.Queries.GetPurchaseReturnPending;

public sealed record GetPurchaseReturnPendingQuery(int PageNumber, int PageSize, string? SearchTerm)
    : IRequest<(IReadOnlyList<PurchaseReturnPendingDto> Items, int Total)>;
