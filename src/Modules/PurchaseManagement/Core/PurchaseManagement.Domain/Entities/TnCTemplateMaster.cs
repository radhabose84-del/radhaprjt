using PurchaseManagement.Domain.Common;

namespace PurchaseManagement.Domain.Entities
{
    public class TnCTemplateMaster : BaseEntity
    {
        public string? TemplateCode { get; set; }
        public string TemplateName { get; set; } = null!;

        // Module (cross-module FK to AppData.Modules — no DB constraint, populated via IModuleLookup)
        public int ModuleId { get; set; }

        // Rich text
        public string TermsHtml { get; set; } = null!;

        // M2M: Applicabilities (RFQ, Quotation, PO, etc.)
        public ICollection<TnCTemplateApplicability>? Applicabilities { get; set; } 
    }
}