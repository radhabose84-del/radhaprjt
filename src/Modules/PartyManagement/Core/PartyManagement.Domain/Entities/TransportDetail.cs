namespace PartyManagement.Domain.Entities
{
    public class TransportDetail
    {
        public int Id { get; set; }
        public int PartyId { get; set; }
        public int? TransporterTypeId { get; set; }
        public int? TransportModeId { get; set; }
        public int? VehicleTypeId { get; set; }
        public int? DefaultFreightTypeId { get; set; }
        public decimal? DefaultFreightRate { get; set; }
        public decimal? MinFreightAmount { get; set; }
        public string? LicenseNo { get; set; }
        public DateTimeOffset? LicenseExpiryDate { get; set; }
        public string? VehicleNo { get; set; }
        public string? InsuranceProvider { get; set; }
        public string? PolicyNo { get; set; }
        public DateTimeOffset? InsuranceExpiryDate { get; set; }
        public byte Status { get; set; } = 1;

        // Navigation properties (same-module FKs)
        public PartyMaster? PartyMaster { get; set; }
        public MiscMaster? TransporterTypeMisc { get; set; }
        public MiscMaster? TransportModeMisc { get; set; }
        public MiscMaster? VehicleTypeMisc { get; set; }
        public MiscMaster? DefaultFreightTypeMisc { get; set; }
    }
}
