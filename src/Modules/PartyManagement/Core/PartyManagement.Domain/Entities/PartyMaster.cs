using PartyManagement.Domain.Common;

namespace PartyManagement.Domain.Entities
{
    public class PartyMaster : BaseEntity
    {
        public string? PartyCode { get; set; }
        public string? PartyName { get; set; }
        public int? PartyZoneId { get; set; }
        public MiscMaster? ZoneType { get; set; } = null!;
        public int? RegistrationTypeId { get; set; }
        public MiscMaster RegistrationType { get; set; } = null!;
        public string? GSTNumber { get; set; }
        public int? GSTStateCode { get; set; }
        public string? PAN { get; set; }
        public string? Website { get; set; }
        public string? TAN { get; set; }
        public int? TDSCategoryId { get; set; }
        public int? MSMETypeId { get; set; }
        public MiscMaster? MSMETypeMisc { get; set; } = null!;
        public string? MSMENO { get; set; }
        public DateTimeOffset? MSMEValidUpto { get; set; }
        public bool IsMsmeCompliant { get; set; }
        public bool IsTDSApplicable { get; set; }
        public bool IsTCSApplicable { get; set; }
        public bool IsGstReverseCharge { get; set; }
        public bool Is206AB206CCAApplicable { get; set; }
        public int? PayementModeId { get; set; }
        public MiscMaster? PaymentModeTypeMisc { get; set; } = null!;
        public string? FavourOf { get; set; }
        public int? PreferredCurrencyPurchase { get; set; }
        public int? CreditDays { get; set; }
        public int? DueDateTypeId { get; set; }
        public MiscMaster? DueDateTypeMisc { get; set; } = null!;
        public int? LeadTime { get; set; }
        public int? PreferredCurrencySale { get; set; }
        public decimal? CreditLimit { get; set; }
        public int? SellingPriceListId { get; set; }
        public int? CustomerTypeId { get; set; }
        public bool IsInternalSupplier { get; set; }
        public bool IsInternalCustomer { get; set; }
        public bool IsStopPayment { get; set; }
        public string? PartyStatus { get; set; }
        public decimal? InsuranceLimit { get; set; }
        public DateTimeOffset? GSTRegistrationDate { get; set; }
        public DateTimeOffset? MSMERegistrationDate { get; set; }
        public string? CIN { get; set; }
        public string? IECode { get; set; }
        public bool IsGroup { get; set; }
        public bool IsSubsidiary { get; set; }
        public int UnitId { get; set; } 
        public int StatusId { get; set; } 
        public bool IsPortalAccessEnabled { get; set; }
        public bool IsUpdate { get; set; }
        public MiscMaster? StatusParty { get; set; }
        public MiscMaster? CustomerTypeMisc { get; set; } = null!;
        public ICollection<PartyContact>? PartyContactTypes { get; set; }
        public ICollection<PartyAddress>? PartyAddressTypes { get; set; }
        public ICollection<PartyType>? PartyTypes { get; set; }
        public ICollection<PartyDocument>? PartyDocumentTypes { get; set; }
        public ICollection<PartyBank>? PartyBankTypes { get; set; }
        public ICollection<PartyUnitCompanyMapping>? PartyUnitCompanyMappings { get; set; }
        public ICollection<SalesType>? SalesTypes { get; set; }

    }
}