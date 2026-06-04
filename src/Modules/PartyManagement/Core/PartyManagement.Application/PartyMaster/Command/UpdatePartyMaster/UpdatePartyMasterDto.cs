namespace PartyManagement.Application.PartyMaster.Command.UpdatePartyMaster
{
    public class UpdatePartyMasterDto
    {
        public int Id { get; set; }
        public string? PartyCode { get; set; } //Readonly Field
        public string? PartyName { get; set; }
        public string? AlternateName { get; set; }
        public string? ShortName { get; set; }
        public byte IsVerified { get; set; }
        public int? StatusControlId { get; set; }
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
        public decimal? InsuranceLimit { get; set; }
        public byte IsActive { get; set; }
        public byte IsPortalAccessEnabled { get; set; }
        public int? SalesFreightId { get; set; }
        public int? PurchaseFreightId { get; set; }
        public string? FreightExpensesGl { get; set; }
        public int? BankAccountId { get; set; }
        public List<UpdatePartyUniCompanyDto>? PartyUnitCompaniesUpdate { get; set; }
        public List<UpdatePartyTypeDto>? PartyTypesUpdate { get; set; }
        public List<UpdatePartyContactDto>? PartyContactsUpdate { get; set; }
        public List<UpdatePartyAddressDto>? PartyAddressesUpdate { get; set; }
        public List<UpdatePartyBankDto>? PartyBanksUpdate { get; set; }
        public List<UpdatePartyDocumentDto>? PartyDocumentsUpdate { get; set; }
        public List<UpdateSalesTypeDto>? SalesTypesUpdate { get; set; }
        public List<UpdateAgentConfigDto>? AgentConfigsUpdate { get; set; }
        public List<UpdateTransportDetailDto>? TransportDetailsUpdate { get; set; }

         public class UpdatePartyUniCompanyDto
        {
            public int Id { get; set; } // PK of PartyType, 0 for new
            public int PartyId { get; set; } // FK 
            public int CompanyId { get; set; }
            public int UnitId { get; set; }
        }
        public class UpdatePartyTypeDto
        {
            public int Id { get; set; } // PK of PartyType, 0 for new
            public int PartyId { get; set; } // FK 
            public int PartyTypeId { get; set; }
            public int PartyGroupId { get; set; }
        }

        public class UpdatePartyContactDto
        {
            public int Id { get; set; } // PK of PartyContact, 0 for new
            public int PartyId { get; set; } // FK 
            public string? FirstName { get; set; }
            public string? LastName { get; set; }
            public int? GenderId { get; set; }
            public string? Designation { get; set; }
            public string? EmailID { get; set; }
            public string? AlternateEmailId { get; set; }
            public string? MobileNo { get; set; }
            public string? AlternateMobileNumber { get; set; }
            public string? Phone { get; set; }
            public int? PreferredChannelId { get; set; }
            public int? ContactTypeId { get; set; }
            public string? ContactBy { get; set; }
        }

        public class UpdatePartyAddressDto
        {
            public int Id { get; set; } // PK of PartyAddress, 0 for new
            public int PartyId { get; set; } // FK 
            public string? AddressType { get; set; }
            public string? AddressLine1 { get; set; }
            public string? AddressLine2 { get; set; }
           // public int? CityId { get; set; }
           // public int? StateId { get; set; }
            public string? PostalCode { get; set; }
           // public int? CountryId { get; set; }
            public string? City { get; set; }
            public string? State { get; set; }
            public string? Country { get; set; }
        }
        public class UpdatePartyBankDto
        {
            public int Id { get; set; } // PK of PartyAddress, 0 for new
            public int PartyId { get; set; } // FK 
            public string? BankName { get; set; }
            public string? BankAccountNumber { get; set; }
            public string? BankBranch { get; set; }
            public string? IFSCCode { get; set; }
            public string? SWIFTCode { get; set; }
            public int? AccountTypeId { get; set; }
            public byte IsDefaultAccount { get; set; }
            public byte IsPrimaryAccount { get; set; }
        }

        public class UpdatePartyDocumentDto
        {
            public int Id { get; set; } // PK of PartDocument, 0 for new
            public int PartyId { get; set; } // FK
            public int DocumentId { get; set; }
            public string? FileName { get; set; }
            public DateTimeOffset UploadedDate { get; set; }
        }

        public class UpdateSalesTypeDto
        {
            public int Id { get; set; } // PK of SalesType, 0 for new
            public int PartyId { get; set; } // FK
            public int? SalesSegmentId { get; set; }
            public int? OrderTypeId { get; set; }
            public int? IncotermId { get; set; }
            public int? PaymentTermsId { get; set; }
            public int? ShippingConditionId { get; set; }
            public int? AccountAssignmentId { get; set; }
            public byte Active { get; set; }
        }

        public class UpdateAgentConfigDto
        {
            public int Id { get; set; } // PK of AgentConfig, 0 for new
            public int PartyId { get; set; } // FK
            public int? SettlementCycleId { get; set; }
            public byte TdsApplicable { get; set; }
            public string? TdsCode { get; set; }
            public string? DefaultCommissionGl { get; set; }
            public DateTimeOffset? AgreementStartDate { get; set; }
            public DateTimeOffset? AgreementEndDate { get; set; }
            public string? AgentPayableControlGl { get; set; }
            public decimal? TargetAmount { get; set; }
            public string? TargetPeriod { get; set; }
            public byte Status { get; set; }
        }

        public class UpdateTransportDetailDto
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
        }

    }
}