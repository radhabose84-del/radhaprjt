namespace FinanceManagement.Application.TaxCode.Dto
{
    public class TaxCodeMasterDto
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }

        public string? TaxCode { get; set; }
        public string? TaxName { get; set; }
        public string? TaxType { get; set; }
        public string? TaxComponent { get; set; }

        public int? ParentTaxCodeId { get; set; }
        public string? ParentTaxCode { get; set; }        // populated via self-join

        public string? Direction { get; set; }
        public string? StatutorySection { get; set; }
        public decimal? ThresholdAmount { get; set; }
        public decimal? ThresholdAggregate { get; set; }
        public string? HsnSacCode { get; set; }

        public bool IsSystemOnlyPosting { get; set; }
        public bool IsEefcRelevant { get; set; }
        public bool IsStatutoryFixed { get; set; }

        // Current (open) rate version
        public decimal? CurrentRatePercent { get; set; }
        public DateOnly? CurrentEffectiveFrom { get; set; }

        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }

        public int CreatedBy { get; set; }
        public DateTimeOffset? CreatedDate { get; set; }
        public string? CreatedByName { get; set; }
        public string? CreatedIP { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTimeOffset? ModifiedDate { get; set; }
        public string? ModifiedByName { get; set; }
        public string? ModifiedIP { get; set; }
    }
}
