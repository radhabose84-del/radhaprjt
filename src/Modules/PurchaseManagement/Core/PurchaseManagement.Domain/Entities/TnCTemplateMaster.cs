using PurchaseManagement.Domain.Common;

namespace PurchaseManagement.Domain.Entities
{
    public class TnCTemplateMaster : BaseEntity
    {
        public string? TemplateCode { get; set; }
        public string TemplateName { get; set; } = null!;

        // Template Type (Purchase/Sales) from Misc
        public int TemplateTypeId { get; set; }
        public MiscMaster? TemplateType { get; set; } 

        // Rich text
        public string TermsHtml { get; set; } = null!;
        public bool? ApprovalFlag { get; set; }

        // M2M: Applicabilities (RFQ, Quotation, PO, etc.)
        public ICollection<TnCTemplateApplicability>? Applicabilities { get; set; } 
    }
}