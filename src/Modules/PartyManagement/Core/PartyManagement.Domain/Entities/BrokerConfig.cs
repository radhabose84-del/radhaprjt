namespace PartyManagement.Domain.Entities
{
    public class BrokerConfig
    {
        public int Id { get; set; }
        public int PartyId { get; set; }
        public PartyMaster Party { get; set; } = null!;
        public int? SettlementCycleId { get; set; }
        public MiscMaster? SettlementCycleMisc { get; set; }
        public byte TdsApplicable { get; set; }
        public string? TdsCode { get; set; }
        public string? DefaultCommissionGl { get; set; }
        public DateTimeOffset? AgreementStartDate { get; set; }
        public DateTimeOffset? AgreementEndDate { get; set; }
        public string? BrokerPayableControlGl { get; set; }
        public decimal? TargetAmount { get; set; }
        public string? TargetPeriod { get; set; }
        public byte Status { get; set; }
    }
}
