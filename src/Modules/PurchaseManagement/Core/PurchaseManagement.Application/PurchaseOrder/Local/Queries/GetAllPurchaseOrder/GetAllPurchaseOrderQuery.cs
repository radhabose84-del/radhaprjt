using PurchaseManagement.Application.Common;
// using PurchaseManagement.Application.PurchaseOrder.Dtos.Local;
using MediatR;
using PurchaseLocalDetailDto.Application.PurchaseOrder.Dtos.Local;

namespace PurchaseManagement.Application.PurchaseOrder.Local.Queries.GetAllPurchaseOrder;

public record GetPurchaseOrdersQuery(int PageNumber, int PageSize, string? SearchTerm,int? PoMethodId,int? StatusId ,int? BudgetGroupId)
    : IRequest<PagedResult<PurchaseOrderListItemDto>>;