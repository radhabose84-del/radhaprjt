namespace FinanceManagement.Application.TaxCode.Dto
{
    public class TaxCodeRateVersionDto
    {
        public int Id { get; set; }
        public int TaxCodeId { get; set; }
        public int VersionNo { get; set; }
        public decimal RatePercent { get; set; }
        public DateOnly EffectiveFrom { get; set; }
        public DateOnly? EffectiveTo { get; set; }
        public string? ChangeReason { get; set; }

        public bool IsActive { get; set; }

        public int CreatedBy { get; set; }
        public DateTimeOffset? CreatedDate { get; set; }
        public string? CreatedByName { get; set; }
    }
}
