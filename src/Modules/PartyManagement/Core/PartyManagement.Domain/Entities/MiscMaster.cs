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
        public ICollection<BankAccount>? BankAccountType { get; set; }
        public ICollection<BankAccount>? BankAccountBranch { get; set; }
        public ICollection<SalesType>? SalesTypeShippingCondition { get; set; }
        public ICollection<SalesType>? SalesTypeAccountAssignment { get; set; }
        public ICollection<PartyMaster>? PartyTransportMode { get; set; }
        public ICollection<PartyMaster>? PartyVehicleType { get; set; }
        public ICollection<PartyMaster>? PartyDefaultFreightType { get; set; }
        public ICollection<AgentConfig>? AgentConfigSettlementCycle { get; set; }

    }
}