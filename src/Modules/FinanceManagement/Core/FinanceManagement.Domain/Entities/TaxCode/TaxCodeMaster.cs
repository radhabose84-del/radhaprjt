using FinanceManagement.Domain.Common;

namespace FinanceManagement.Domain.Entities
{
    // US-GL02-05A — tax-code catalogue header. Rate is NOT stored here; it lives in
    // TaxCodeRateVersion so a rate change writes a new effective-dated row (AC3-A).
    public class TaxCodeMaster : BaseEntity, IActivityTracked
    {
        public int CompanyId { get; set; }

        public string? TaxCode { get; set; }              // e.g. GST-OUT-5 (immutable after create)
        public string? TaxName { get; set; }
        public int TaxTypeId { get; set; }                // FK -> MiscMaster (TAX TYPE): GST_IN/GST_OUT/IGST/TDS/CUSTOMS
        public int? TaxComponentId { get; set; }          // FK -> MiscMaster (TAX COMPONENT): COMBINED/CGST/SGST/IGST/CESS/NA
        public int? DirectionId { get; set; }             // FK -> MiscMaster (TAX DIRECTION): INPUT/OUTPUT
        public int? ParentTaxCodeId { get; set; }         // self-FK -> combined header (component children only)

        public string? StatutorySection { get; set; }      // TDS section (legacy 194x placeholder pending IT Act 2025)
        public decimal? ThresholdAmount { get; set; }      // TDS single-transaction threshold
        public decimal? ThresholdAggregate { get; set; }   // TDS annual-aggregate threshold
        public string? HsnSacCode { get; set; }

        public bool IsSystemOnlyPosting { get; set; }      // blocks manual JV posting (194C / GST / TDS control)
        public bool IsEefcRelevant { get; set; }           // EEFC / exchange-earner flag
        public bool IsStatutoryFixed { get; set; }         // statutory-fixed vs config

        // Self-referencing navigation (combined header <-> component children)
        public TaxCodeMaster? ParentTaxCode { get; set; }
        public ICollection<TaxCodeMaster>? ComponentCodes { get; set; }

        // MiscMaster lookups (same-module)
        public MiscMaster? TaxTypeMaster { get; set; }
        public MiscMaster? TaxComponentMaster { get; set; }
        public MiscMaster? DirectionMaster { get; set; }

        // Same-module FK navigations
        public ICollection<TaxCodeRateVersion>? RateVersions { get; set; }
        public ICollection<TaxAccountLinkage>? Linkages { get; set; }
    }
}
