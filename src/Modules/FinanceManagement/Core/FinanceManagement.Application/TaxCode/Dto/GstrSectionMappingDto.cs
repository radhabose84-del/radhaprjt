namespace FinanceManagement.Application.TaxCode.Dto
{
    public class GstrSectionMappingDto
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public string? GstrType { get; set; }
        public string? SectionCode { get; set; }
        public string? SectionName { get; set; }
        public string? AccountRangeFrom { get; set; }
        public string? AccountRangeTo { get; set; }
        public decimal? TolerancePercent { get; set; }

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
