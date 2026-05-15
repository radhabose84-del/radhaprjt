namespace PurchaseManagement.Application.PurchaseOrder.Dtos.Local;

public sealed class PurchaseValueTotalDto
{
    public decimal TotalPurchaseValue { get; set; }
    public int? BudgetGroupId { get; set; }
    public int? ItemCategoryId { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }
}
