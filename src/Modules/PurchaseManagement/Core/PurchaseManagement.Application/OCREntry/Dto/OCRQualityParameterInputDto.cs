namespace PurchaseManagement.Application.OCREntry.Dto
{
    /// <summary>
    /// One cotton-quality parameter value submitted with an OCR Create/Update command.
    /// </summary>
    public sealed class OCRQualityParameterInputDto
    {
        public int ParamId { get; set; }     // QC.QualityParameter.Id
        public string? Value { get; set; }
    }
}
