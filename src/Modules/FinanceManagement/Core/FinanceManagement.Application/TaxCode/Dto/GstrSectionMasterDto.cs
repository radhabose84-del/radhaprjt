namespace FinanceManagement.Application.TaxCode.Dto
{
    public class GstrSectionMasterDto
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }

        public int ReportTypeId { get; set; }
        public string? ReportType { get; set; }              // MiscMaster code (join): GSTR-1 / GSTR-3B

        public string? SectionCode { get; set; }
        public string? SectionName { get; set; }

        public bool IsActive { get; set; }

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
