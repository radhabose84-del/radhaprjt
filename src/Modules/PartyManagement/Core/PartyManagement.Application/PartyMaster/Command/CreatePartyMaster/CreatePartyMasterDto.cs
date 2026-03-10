namespace PartyManagement.Application.PartyMaster.Command.CreatePartyMaster
{
    public class CreatePartyMasterDto
    {
       // public int CompanyId { get; set; }
      //  public string? PartyCode { get; set; }
        public string? PartyName { get; set; }
        public int? PartyZoneId { get; set; }
        public int? RegistrationTypeId { get; set; }
        public string? GSTNumber { get; set; }
        public int? GSTStateCode { get; set; }
        public string? PAN { get; set; }
        public string? Website { get; set; }
        public string? TAN { get; set; }
        public int? TDSCategoryId { get; set; }
        public int? MSMETypeId { get; set; }
        public string? MSMENO { get; set; }
        public DateTimeOffset? MSMEValidUpto { get; set; }
        public byte IsMsmeCompliant { get; set; }
        public byte IsTDSApplicable { get; set; }
        public byte IsTCSApplicable { get; set; }
        public byte IsGstReverseCharge { get; set; }
        public byte Is206AB206CCAApplicable { get; set; }
        public int? PayementModeId { get; set; }
        public string? FavourOf { get; set; }
        public int? PreferredCurrencyPurchase { get; set; }
        public int? CreditDays { get; set; }
        public int? DueDateTypeId { get; set; }
        public int? LeadTime { get; set; }
        public int? PreferredCurrencySale { get; set; }
        public decimal? CreditLimit { get; set; }
        public int? SellingPriceListId { get; set; }
        public int? CustomerTypeId { get; set; }
        public byte IsInternalSupplier { get; set; }
        public byte IsInternalCustomer { get; set; }
        public byte IsStopPayment { get; set; }
        public DateTimeOffset? GSTRegistrationDate { get; set; }
        public DateTimeOffset? MSMERegistrationDate { get; set; }
        public string? CIN { get; set; }
        public string? IECode { get; set; }
        public byte IsGroup { get; set; }
        public byte IsSubsidiary { get; set; }
        public int UnitId { get; set; }
        public decimal? InsuranceLimit { get; set; }
        public byte IsPortalAccessEnabled { get; set; }

        public List<PartyUnitCompanyDto>? PartyUnitCompanies { get; set; } 
        public List<PartyTypeDto>? PartyTypes { get; set; }
        public List<PartyContactDto>? PartyContacts { get; set; }
        public List<PartyAddressDto>? PartyAddresses { get; set; }
        public List<PartyBankDto>? PartyBanks { get; set; }
        public List<PartyDocumentDto>? PartyDocuments { get; set; }
        public List<SalesTypeDto>? SalesTypes { get; set; }

        public class PartyUnitCompanyDto
        {
            public int CompanyId { get; set; } 
            public int UnitId { get; set; } 

        }

        public class PartyContactDto
        {
            public string? FirstName { get; set; }
            public string? LastName { get; set; }
            public int? GenderId { get; set; }
            public string? Designation { get; set; }
            public string? EmailID { get; set; }
            public string? MobileNo { get; set; }
            public string? Phone { get; set; }
            public int? PreferredChannelId { get; set; }
            public int? ContactTypeId { get; set; }
            public string? ContactBy { get; set; }
        }
        public class PartyAddressDto
        {
            public string? AddressType { get; set; }
            public string? AddressLine1 { get; set; }
            public string? AddressLine2 { get; set; }
            public string? City { get; set; }
            public string? State { get; set; }
            public string? PostalCode { get; set; }
            public string? Country { get; set; }
            // public int CityId { get; set; }
            // public int StateId { get; set; }
            // public int CountryId { get; set; }
        }

        public class PartyBankDto
        {
            public string? BankName { get; set; }
            public string? BankAccountNumber { get; set; }
            public string? BankBranch { get; set; }
            public string? IFSCCode { get; set; }
            public string? SWIFTCode { get; set; }
            public int? AccountTypeId { get; set; }
            public byte IsDefaultAccount { get; set; }
            public byte IsPrimaryAccount { get; set; }
        }

        public class PartyTypeDto
        {
            public int PartyTypeId { get; set; } 
            public int PartyGroupId { get; set; } 

        }

        public class PartyDocumentDto
        {
            public int DocumentId { get; set; }
            public string? FileName { get; set; }
            public DateTimeOffset UploadedDate { get; set; }


        }

        public class SalesTypeDto
        {
            public int? SalesSegmentId { get; set; }
            public int? OrderTypeId { get; set; }
            public int? IncotermId { get; set; }
            public int? PaymentTermsId { get; set; }
            public int? ShippingConditionId { get; set; }
            public int? AccountAssignmentId { get; set; }
            public byte Active { get; set; }
        }

    }
}