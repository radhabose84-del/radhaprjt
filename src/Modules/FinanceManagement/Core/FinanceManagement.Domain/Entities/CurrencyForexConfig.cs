using FinanceManagement.Domain.Common;

namespace FinanceManagement.Domain.Entities
{
    /// <summary>
    /// Currency / forex configuration master (US-GL02-12).
    /// A GL account references one row of this master via its single "Currency Type" dropdown
    /// (GlAccountMaster.CurrencyTypeId). Today it carries only the currency-type classification;
    /// EEFC, revaluation-account mapping and allowed currency are added when their stories land (GL-04).
    /// </summary>
    public class CurrencyForexConfig : BaseEntity
    {
        public int CompanyId { get; set; }

        public string? CurrencyTypeCode { get; set; }   // e.g. INRONLY / FOREX / MULTICUR (immutable)
        public string? CurrencyTypeName { get; set; }   // e.g. INR-only / Forex / Multi-currency
    }
}
