using MediatR;
using PurchaseManagement.Application.PurchaseOrder.Dtos.Local;

namespace PurchaseManagement.Application.PurchaseOrder.Local.Queries.GetTotalPurchaseValue;

public record GetTotalPurchaseValueQuery(
    int? BudgetGroupId,
    int? ItemCategoryId,
    DateTimeOffset Date)
    : IRequest<PurchaseValueTotalDto>;
