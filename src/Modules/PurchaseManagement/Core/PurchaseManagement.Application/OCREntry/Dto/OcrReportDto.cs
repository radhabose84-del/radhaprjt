namespace PurchaseManagement.Application.OCREntry.Dto
{
    /// <summary>
    /// Dynamic, print-ready representation of an OCR (Order Confirmation Report). The frontend
    /// binds each field by its stable <see cref="OcrReportField.Key"/>, prints <see cref="OcrReportField.Label"/>,
    /// and positions sections into the fixed PDF layout.
    /// </summary>
    public sealed class OcrReportDto
    {
        public List<OcrReportSection> Sections { get; set; } = new();
    }

    public sealed class OcrReportSection
    {
        public string Key { get; set; } = default!;
        public string Title { get; set; } = default!;
        public List<OcrReportField> Fields { get; set; } = new();
    }

    public sealed class OcrReportField
    {
        public string Key { get; set; } = default!;
        public string Label { get; set; } = default!;

        /// <summary>Print-ready string (units baked in, dates dd.MM.yy). Empty string when blank.</summary>
        public string Value { get; set; } = string.Empty;

        /// <summary>Optional raw value (number / ISO date) for alignment or sorting on the UI.</summary>
        public object? Raw { get; set; }
    }
}
