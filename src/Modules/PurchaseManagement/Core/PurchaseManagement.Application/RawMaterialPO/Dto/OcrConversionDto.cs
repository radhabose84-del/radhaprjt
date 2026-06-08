using PurchaseManagement.Application.OCREntry.Dto;

namespace PurchaseManagement.Application.RawMaterialPO.Dto
{
    /// <summary>
    /// Auto-fetched OCR details for the conversion screen, plus the cumulative conversion progress
    /// so the UI can enforce the "Max N Bales" cap (RemainingQuantity).
    /// </summary>
    public class OcrConversionDto
    {
        public OCREntryDto? Ocr { get; set; }
        public decimal ConvertedQuantity { get; set; }
        public decimal RemainingQuantity { get; set; }
        public string ConversionStatus { get; set; } = "Not Converted";
    }
}
