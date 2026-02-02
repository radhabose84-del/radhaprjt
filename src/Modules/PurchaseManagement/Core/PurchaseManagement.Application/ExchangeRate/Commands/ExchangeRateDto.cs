namespace PurchaseManagement.Application.ExchangeRate.Commands;

public sealed class ExchangeRateDto
{
    public string BaseCurrency { get; set; } = default!;
    public string QuoteCurrency { get; set; } = default!;
    public decimal Rate { get; set; }
    public decimal? ActualRate { get; set; }
    public DateOnly EffectiveDate { get; set; }
    public string Source { get; set; } = "Frankfurter";
    public int Id { get; set; }
}

