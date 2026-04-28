using SalesManagement.Domain.Common;

namespace SalesManagement.Domain.Entities
{
    public class SalesOrderTypeMaster : BaseEntity
    {
        public int SoTypeId { get; set; }                       // Same-module FK → MiscMaster (MiscType=SOTM_TYPE)
        public int TaxTypeId { get; set; }                      // Cross-module FK → Finance.TransactionTypeMaster (NO DB FK)

        public string? TypeName { get; set; }
        public string? Description { get; set; }

        // Type behavior
        public bool AllowsDispatch { get; set; } = true;
        public bool RequiresValidity { get; set; }
        public bool AllowZeroPrice { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public decimal? MaxQty { get; set; }
        public bool AllowPriceOverride { get; set; }
        public decimal? OverrideLimitPercent { get; set; }
        public bool ApprovalRequired { get; set; }

        // Mode behavior (Sales_Mode + Tax_Type derived from TaxTypeId at runtime — not stored)
        public bool CurrencyRequired { get; set; }
        public bool AllowIGST { get; set; }
        public bool CountryMandatory { get; set; }
        public int? DefaultCurrencyId { get; set; }             // Cross-module FK → AppData.Currency (NO DB FK)

        // Navigation (same-module only)
        public MiscMaster? SoType { get; set; }

        // Reverse navigation (SalesOrder)
        public ICollection<SalesOrderHeader>? SalesOrderHeaders { get; set; }
    }
}
