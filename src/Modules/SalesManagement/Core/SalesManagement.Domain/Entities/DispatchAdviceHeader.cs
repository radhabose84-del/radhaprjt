using SalesManagement.Domain.Common;

namespace SalesManagement.Domain.Entities
{
    public class DispatchAdviceHeader : BaseEntity
    {
        public string? DispatchNo { get; set; }
        public DateOnly DispatchDate { get; set; }
        public int StatusId { get; set; }                    // FK → Sales.MiscMaster (Draft/Posted/Cancelled)
        public int SalesOrderId { get; set; }               // FK → Sales.SalesOrderHeader
        public int PartyId { get; set; }                    // Cross-module FK → PartyManagement
        public decimal TotOrderQty { get; set; }
        public decimal TotDispatchedQty { get; set; }
        public decimal TotPendingQty { get; set; }
        public int? DispatchAddressId { get; set; }          // FK → Sales.DispatchAddressMaster
        public int DispatchTypeId { get; set; }             // FK → Sales.MiscMaster
        public int FreightId { get; set; }                  // Cross-module FK → Logistics.FreightMaster
        public int? TransporterId { get; set; }             // Cross-module FK → PartyManagement (optional)
        public string? VehicleNo { get; set; }
        public string? DriverName { get; set; }
        public string? LRNo { get; set; }
        public int UnitId { get; set; }                    // Cross-module FK → UserManagement
        public bool InvFlg { get; set; }                   // Invoice flag: false=N, true=Y
        public decimal Distance { get; set; }              // Distance in km between FROM (unit pincode) and TO (party pincode)

        // Same-module navigation properties
        public MiscMaster? StatusMisc { get; set; }
        public MiscMaster? DispatchTypeMisc { get; set; }
        public SalesOrderHeader? SalesOrderHeader { get; set; }
        public DispatchAddressMaster? DispatchAddress { get; set; }

        // Child collection
        public ICollection<DispatchAdviceDetail>? DispatchAdviceDetails { get; set; }

        // Reverse navigation (InvoiceHeader)
        public ICollection<InvoiceHeader>? InvoiceHeaders { get; set; }
    }
}
