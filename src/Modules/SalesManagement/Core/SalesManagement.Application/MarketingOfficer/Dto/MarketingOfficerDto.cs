namespace SalesManagement.Application.MarketingOfficer.Dto
{
    public class MarketingOfficerDto
    {
        public int Id { get; set; }
        public string? EmployeeNo { get; set; }
        public string? EmployeeName { get; set; }
        public string? MobileNo { get; set; }
        public string? Email { get; set; }
        public string? Unit { get; set; }
        public string? Department { get; set; }
        public string? Designation { get; set; }
        public int SalesOfficeId { get; set; }
        public string? SalesOfficeName { get; set; }
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
        public List<OfficerSalesGroupDto> SalesGroups { get; set; } = new();
    }
}
