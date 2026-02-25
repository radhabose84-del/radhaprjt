using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities.GRN.GRNEntry;


namespace PurchaseManagement.Domain.Entities.GRN.GateEntry
{
    public class GateEntryHeader : BaseEntity
    {
        public string? GateEntryNo { get; set; }
        public DateTimeOffset GateEntryDate { get; set; }
        public int UnitId { get; set; }
        public int PartyId { get; set; }
        public string VehicleNumber { get; set; } = string.Empty;
        public string? DriverName { get; set; }
        public decimal? GrossWeight { get; set; }
        public decimal? TareWeight { get; set; }
        public decimal? NetWeight { get; set; }
        public string? ImagePath { get; set; }
        public string? Remarks { get; set; }
        public int ReceivingTypeId { get; set; }
        public MiscMaster? GateEntryReceivingType { get; set; }
        public ICollection<GateEntryDetail>? GateEntryDetails { get; set; }
        public ICollection<GrnHeader>? GrnGateEntries { get; set; }
        
        
    }
}