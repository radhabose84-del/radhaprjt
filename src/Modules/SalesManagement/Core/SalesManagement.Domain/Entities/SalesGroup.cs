using SalesManagement.Domain.Common;

namespace SalesManagement.Domain.Entities
{
    public class SalesGroup : BaseEntity
    {
        public string? SalesGroupName { get; set; }
        public int SalesOfficeId { get; set; }
        public string? ResponsibleManager { get; set; }
        public int? ProductCategoryId { get; set; }
        public string? RegionTerritory { get; set; }

        // Navigation properties
        public SalesOffice? SalesOffice { get; set; }
        public ICollection<OfficerSalesGroup>? OfficerSalesGroups { get; set; }

        // Reverse navigation
        public ICollection<SalesOrderHeader>? SalesOrderHeaders { get; set; }
        public ICollection<DiscountSalesGroup>? DiscountSalesGroups { get; set; }
        public ICollection<AgentCommissionSalesGroup>? AgentCommissionSalesGroups { get; set; }
        public ICollection<SalesAgreementHeader>? SalesAgreementHeaders { get; set; }
    }
}
