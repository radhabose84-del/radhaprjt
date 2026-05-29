namespace PurchaseManagement.Application.PurchaseOrder.Dtos.BlanketPO;

public class BlanketPOCreateDto
{
    // ─── Blanket Reference ──────────────────────
    public int BlanketHeaderId { get; set; }

    // ─── PO Header Fields ─────────────────────────
    public DateTimeOffset PODate { get; set; }
    public int POCategoryId { get; set; }
    public int? POMethodId { get; set; }

    // ─── PO Totals (calculated frontend → backend) ──
    public decimal ItemTotal { get; set; }
    public decimal? DiscountTotal { get; set; }
    public decimal? PandFTotal { get; set; }
    public decimal? MiscCharges { get; set; }
    public decimal GSTTotal { get; set; }
    public decimal? CGSTTotal { get; set; }
    public decimal? SGSTTotal { get; set; }
    public decimal? IGSTTotal { get; set; }
    public decimal? FreightTotal { get; set; }
    public decimal PurchaseValue { get; set; }

    // ─── PurchaseBlanketHeader Fields ───────────
    public bool IsPartialReceiptAllowed { get; set; }
    public int? IncotermsId { get; set; }
    public int? ModeOfDispatchId { get; set; }
    public decimal? FreightCharges { get; set; }
    public int? TermsId { get; set; }
    public string? TermDescription { get; set; }
    public string? DeliveryAddress { get; set; }
    public string? BillingAddress { get; set; }

    // ─── Cost Centre & Budget ────────────────────
    public int? CostCenterId { get; set; }
    public int? BudgetGroupId { get; set; }
    public int? BudgetMonthId { get; set; }
    public int? BudgetRequestById { get; set; }
    public int? ProjectId { get; set; }
    public int? WBSId { get; set; }
    public int? FinancialYearId { get; set; }

    // ─── Release Details ─────────────────────────
    public List<BlanketPODetailItem> Details { get; set; } = new();
    public List<BlanketPOPaymentTermItem> PaymentTerms { get; set; } = new();
}
