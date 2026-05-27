using MediatR;
using PurchaseManagement.Application.Common;
using PurchaseManagement.Application.PurchaseReturn.PurchaseReturn.Dto;

namespace PurchaseManagement.Application.PurchaseReturn.PurchaseReturn.Queries.GetAllPurchaseReturns;

public sealed record GetAllPurchaseReturnsQuery(int PageNumber, int PageSize, string? SearchTerm)
    : IRequest<PagedResult<PurchaseReturnListItemDto>>;
