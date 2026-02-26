using SalesManagement.Domain.Common;

namespace SalesManagement.Domain.Entities
{
    public class MarketingOfficer : BaseEntity
    {
        public string EmployeeNo { get; set; } = null!;
        public string EmployeeName { get; set; } = null!;
        public string? MobileNo { get; set; }
        public string? Email { get; set; }
        public string Unit { get; set; } = null!;
        public string Department { get; set; } = null!;
        public string Designation { get; set; } = null!;
        public int SalesOfficeId { get; set; }

        // Navigation properties
        public SalesOffice SalesOffice { get; set; } = null!;
        public ICollection<OfficerSalesGroup> OfficerSalesGroups { get; set; } = null!;
    }
}
