using SalesManagement.Domain.Common;

namespace SalesManagement.Domain.Entities
{
    public class InvoiceHeader : BaseEntity
    {
        public string? InvoiceNo { get; set; }
        public DateOnly InvoiceDate { get; set; }
        public int DispatchAdviceId { get; set; }         // FK → Sales.DispatchAdviceHeader
        public int PartyId { get; set; }                  // Cross-module FK → PartyManagement
        public int? AgentId { get; set; }                 // FK → Sales.OfficerAgent
        public int UnitId { get; set; }                   // Cross-module FK → UserManagement
        public int FinancialYearId { get; set; }          // Cross-module FK → UserManagement
        public int? TransportMode { get; set; }           // FK → Sales.MiscMaster
        public int? StatusId { get; set; }                // FK → Sales.MiscMaster
        public string? VehicleNumber { get; set; }
        public string? TransporterName { get; set; }
        public string? LRNumber { get; set; }
        public DateOnly? LRDate { get; set; }
        public int TotalBags { get; set; }
        public decimal TotalWeight { get; set; }
        public decimal TaxableValue { get; set; }
        public decimal Discount { get; set; }
        public decimal Freight { get; set; }
        public decimal Insurance { get; set; }
        public decimal HandlingCharge { get; set; }
        public decimal TotalCharity { get; set; }
        public decimal OtherCharges { get; set; }
        public decimal CGST { get; set; }
        public decimal SGST { get; set; }
        public decimal IGST { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TCSPercentage { get; set; }
        public decimal TCS { get; set; }
        public decimal RoundOff { get; set; }
        public decimal InvoiceAmountBeforeTCS { get; set; }
        public decimal InvoiceAmount { get; set; }
        public string? Remarks { get; set; }
        public bool GEFlag { get; set; }

        // Same-module navigation properties
        public MiscMaster? TransportModeMisc { get; set; }
        public MiscMaster? StatusMisc { get; set; }
        public DispatchAdviceHeader? DispatchAdviceHeader { get; set; }

        // Child collection
        public ICollection<InvoiceDetail>? InvoiceDetails { get; set; }
    }
}
