using SalesManagement.Domain.Common;

namespace SalesManagement.Domain.Entities
{
    public class SalesOrderHeader : BaseEntity
    {
        public string? SalesOrderNo { get; set; }
        public DateOnly OrderDate { get; set; }

        // Customer & Unit Details
        public int SalesGroupId { get; set; }
        public int? SalesSegmentId { get; set; }
        public int EnquiryType { get; set; }            // 1=Unit, 2=Combined
        public int UnitId { get; set; }                  // Cross-module FK (UserManagement)
        public int PartyId { get; set; }                 // Cross-module FK (PartyManagement)

        // Commercial Details
        public int? DiscountPlanId { get; set; }
        public int PaymentTermsId { get; set; }          // Cross-module FK (PurchaseManagement)
        public int? PaymentTypeId { get; set; }
        public int FreightTypeId { get; set; }
        public int? CountListId { get; set; }
        public string? Remarks { get; set; }

        // File Attachments
        public string? VisitNotesAttachment { get; set; }
        public string? AgentPOAttachment { get; set; }

        // Dispatch Location
        public int DispatchLocationType { get; set; }    // 1=Depot, 2=Unit
        public int? DispatchDepotId { get; set; }        // Cross-module FK (WarehouseManagement)
        public int? DispatchUnitId { get; set; }         // Cross-module FK (UserManagement)

        // Derived Summary Fields
        public int TotalBags { get; set; }
        public decimal TotalWeightKgs { get; set; }
        public decimal TotalDiscountPerKg { get; set; }
        public decimal ItemValue { get; set; }
        public decimal TotalFreight { get; set; }
        public decimal TaxableAmount { get; set; }
        public decimal GSTPercentage { get; set; }
        public decimal TotalGST { get; set; }
        public decimal TotalWithGST { get; set; }
        public decimal TCSPercentage { get; set; }
        public decimal TotalTCS { get; set; }
        public decimal FinalAmount { get; set; }

        // Navigation Properties (Same-Module FKs only)
        public SalesGroup? SalesGroup { get; set; }
        public SalesSegment? SalesSegment { get; set; }
        public MiscMaster? DiscountPlan { get; set; }
        public MiscMaster? PaymentType { get; set; }
        public MiscMaster? FreightType { get; set; }
        public MiscMaster? CountList { get; set; }

        // Child collection
        public ICollection<SalesOrderDetail>? SalesOrderDetails { get; set; }
    }
}
