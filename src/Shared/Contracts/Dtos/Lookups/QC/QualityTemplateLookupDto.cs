namespace Contracts.Dtos.Lookups.QC
{
    /// <summary>
    /// Cross-module lookup row for a QC Quality Template header
    /// (Qc.QualityTemplate). Used to populate the template name on an OCR Entry
    /// and to validate template existence.
    /// </summary>
    public sealed class QualityTemplateLookupDto
    {
        public int Id { get; set; }
        public string? TemplateCode { get; set; }
        public string? TemplateName { get; set; }
    }
}
