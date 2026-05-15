using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.Local;
using PurchaseManagement.Application.PurchaseOrder.Dtos.Local;

namespace PurchaseManagement.Application.PurchaseOrder.Local.Queries.GetTotalPurchaseValue;

public class GetTotalPurchaseValueQueryHandler
    : IRequestHandler<GetTotalPurchaseValueQuery, PurchaseValueTotalDto>
{
    private readonly IPurchaseOrderQueryRepository _repo;

    public GetTotalPurchaseValueQueryHandler(IPurchaseOrderQueryRepository repo)
    {
        _repo = repo;
    }

    public async Task<PurchaseValueTotalDto> Handle(
        GetTotalPurchaseValueQuery request, CancellationToken ct)
    {
        var total = await _repo.GetTotalPurchaseValueAsync(
            request.BudgetGroupId,
            request.ItemCategoryId,
            request.Date,
            ct);

        return new PurchaseValueTotalDto
        {
            TotalPurchaseValue = total,
            BudgetGroupId = request.BudgetGroupId,
            ItemCategoryId = request.ItemCategoryId,
            Month = request.Date.Month,
            Year = request.Date.Year
        };
    }
}
