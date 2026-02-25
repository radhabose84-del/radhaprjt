namespace PurchaseManagement.Application.TnCTemplateMaster.Queries.GetAllTnCTemplateMaster
{
    public class TncTemplateMasterDto
    {
         public int Id { get; set; }
    public string? TemplateCode { get; set; }
    public string TemplateName { get; set; } = null!;
    public int TemplateTypeId { get; set; }
    public string TemplateTypeCode { get; set; } = null!;
    public string TemplateTypeDescription { get; set; } = null!;
    public string? TermsHtml { get; set; }
    public bool? ApprovalFlag { get; set; }
    public bool IsActive { get; set; }
    public DateTimeOffset? CreatedDate { get; set; }

    public List<TncApplicabilityDto> Applicabilities { get; set; } = new();
    }
}