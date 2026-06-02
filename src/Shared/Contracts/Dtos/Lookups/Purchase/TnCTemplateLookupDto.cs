namespace Contracts.Dtos.Lookups.Purchase
{
    public sealed class TnCTemplateLookupDto
    {
        public int Id { get; set; }
        public string? TemplateCode { get; set; }
        public string? TemplateName { get; set; }
        public string? TermsHtml { get; set; }
    }
}
