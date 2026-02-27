using SalesManagement.Domain.Common;

namespace SalesManagement.Domain.Entities
{
    public class MarketingOfficer : BaseEntity
    {
        public string? EmployeeNo { get; set; }
        public string? EmployeeName { get; set; }
        public string? MobileNo { get; set; }
        public string? Email { get; set; }
        public string? Unit { get; set; }
        public string? Department { get; set; }
        public string? Designation { get; set; }
        public int SalesOfficeId { get; set; }

        // Navigation properties
        public SalesOffice? SalesOffice { get; set; }
        public ICollection<OfficerSalesGroup>? OfficerSalesGroups { get; set; }
    }
}
