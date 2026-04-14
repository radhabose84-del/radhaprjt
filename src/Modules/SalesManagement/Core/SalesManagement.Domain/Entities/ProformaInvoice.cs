using SalesManagement.Domain.Common;

namespace SalesManagement.Domain.Entities
{
    public class ProformaInvoice : BaseEntity
    {
        public string? ProformaNumber { get; set; }
        public DateOnly ProformaDate { get; set; }
        public int SalesOrderId { get; set; }
        public int PartyId { get; set; }
        public decimal ProformaAmount { get; set; }
        public decimal SOBalance { get; set; }
        public decimal PaymentReceivedAmount { get; set; }
        public int? StatusId { get; set; }
        public string? Remarks { get; set; }

        // Same-module navigation properties
        public SalesOrderHeader? SalesOrderHeader { get; set; }
        public MiscMaster? StatusMisc { get; set; }
    }
}
