namespace SalesManagement.Application.SalesOrderTypeMaster.Dto
{
    public class SalesOrderTypeMasterDto
    {
        public int Id { get; set; }

        // Identification
        public int SoTypeId { get; set; }
        public string? SoTypeCode { get; set; }                  // populated via JOIN to MiscMaster
        public string? SoTypeName { get; set; }                  // populated via JOIN to MiscMaster
        public int TaxTypeId { get; set; }
        public string? TaxTypeName { get; set; }                 // populated via ITransactionTypeLookup
        public string? TaxTypeShortName { get; set; }            // populated via ITransactionTypeLookup
        public string? TypeName { get; set; }
        public string? Description { get; set; }

        // Type behavior
        public bool AllowsDispatch { get; set; }
        public bool RequiresValidity { get; set; }
        public bool AllowZeroPrice { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public decimal? MaxQty { get; set; }
        public bool AllowPriceOverride { get; set; }
        public decimal? OverrideLimitPercent { get; set; }
        public bool ApprovalRequired { get; set; }

        // Mode behavior
        public bool CurrencyRequired { get; set; }
        public bool AllowIGST { get; set; }
        public bool CountryMandatory { get; set; }
        public int? DefaultCurrencyId { get; set; }
        public string? DefaultCurrencyCode { get; set; }         // populated via ICurrencyLookup

        // BaseEntity
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public int CreatedBy { get; set; }
        public DateTimeOffset? CreatedDate { get; set; }
        public string? CreatedByName { get; set; }
        public string? CreatedIP { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTimeOffset? ModifiedDate { get; set; }
        public string? ModifiedByName { get; set; }
        public string? ModifiedIP { get; set; }
    }
}
