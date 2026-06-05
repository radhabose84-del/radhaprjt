namespace PurchaseManagement.Application.OCREntry.Dto
{
    /// <summary>
    /// A stored cotton-quality parameter value for an OCR, with the parameter code/name
    /// resolved via the QC quality-template lookup on read.
    /// </summary>
    public sealed class OCRQualityParameterDto
    {
        public int Id { get; set; }
        public int ParamId { get; set; }
        public string? ParameterCode { get; set; }
        public string? ParameterName { get; set; }
        public string? Value { get; set; }
    }
}
