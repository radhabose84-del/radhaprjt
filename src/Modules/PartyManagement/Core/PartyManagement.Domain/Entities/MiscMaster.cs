using PartyManagement.Domain.Common;

namespace PartyManagement.Domain.Entities
{
    public class MiscMaster : BaseEntity
    {
        public int MiscTypeId { get; set; }
        public string? Code { get; set; }
        public string? Description { get; set; }
        public int SortOrder { get; set; }
        public MiscTypeMaster? MiscTypeMaster { get; set; }
        public ICollection<PartyMaster>? PartyZoneType { get; set; }
        public ICollection<PartyMaster>? PartyMSMEType { get; set; }
        public ICollection<PartyMaster>? PartyPaymentModeType { get; set; }
        public ICollection<PartyMaster>? PartyDueDateType { get; set; }
        public ICollection<PartyMaster>? PartyCustomerType { get; set; }
        public ICollection<PartyGroup>? PartyGroupTypes { get; set; }
        public ICollection<PartyGroup>? PartyGlCategoryCode { get; set; }
        public ICollection<PartyMaster>? PartyRegistrationType { get; set; }
        public ICollection<PartyContact>? PartyGender { get; set; }
        public ICollection<PartyContact>? ContactPreferredChannel { get; set; }
        public ICollection<PartyContact>? PartyContactType { get; set; }
        public ICollection<PartyType>? PartyTypeGroup { get; set; }
        public ICollection<PartyDocument>? PartyDocumentType { get; set; }
        public ICollection<PartyBank>? PartyBankType { get; set; }
        public ICollection<PartyMaster>? StatusHeader { get; set; }
        public ICollection<PartyMaster>? StatusControlHeader { get; set; }
        public ICollection<BankAccount>? BankAccountType { get; set; }
        public ICollection<BankAccount>? BankAccountBranch { get; set; }
        public ICollection<BankAccount>? BankAccountOwnerType { get; set; }
        public ICollection<SalesType>? SalesTypeShippingCondition { get; set; }
        public ICollection<SalesType>? SalesTypeAccountAssignment { get; set; }
        public ICollection<TransportDetail>? TransportDetailTransporterType { get; set; }
        public ICollection<TransportDetail>? TransportDetailTransportMode { get; set; }
        public ICollection<TransportDetail>? TransportDetailVehicleType { get; set; }
        public ICollection<TransportDetail>? TransportDetailDefaultFreightType { get; set; }
        // Reverse-nav for the optional FK DefaultProcurementRateBasisId — collection is nullable,
        // matching the per-row nav nullability on TransportDetail.
        public ICollection<TransportDetail>? TransportDetailDefaultProcurementRateBasis { get; set; }
        public ICollection<AgentConfig>? AgentConfigSettlementCycle { get; set; }
        public ICollection<BrokerConfig>? BrokerConfigSettlementCycle { get; set; }

    }
}