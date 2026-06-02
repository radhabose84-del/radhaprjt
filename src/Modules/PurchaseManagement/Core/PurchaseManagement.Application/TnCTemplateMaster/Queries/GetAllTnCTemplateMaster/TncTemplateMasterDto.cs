namespace PurchaseManagement.Application.TnCTemplateMaster.Queries.GetAllTnCTemplateMaster
{
    public class TncTemplateMasterDto
    {
         public int Id { get; set; }
    public string? TemplateCode { get; set; }
    public string TemplateName { get; set; } = null!;
    public int ModuleId { get; set; }
    public string? ModuleName { get; set; }   // populated via IModuleLookup
    public string? TermsHtml { get; set; }
    public bool IsActive { get; set; }
    public DateTimeOffset? CreatedDate { get; set; }

    public List<TncApplicabilityDto> Applicabilities { get; set; } = new();
    }
}