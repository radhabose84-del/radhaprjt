namespace PurchaseManagement.Application.BarcodeSeries.Dto
{
    public class BarcodeSeriesDto
    {
        public int Id { get; set; }
        public string? BarcodeSeriesNumber { get; set; }
        public int PrefixId { get; set; }
        public string? Prefix { get; set; }
        public DateTimeOffset GenerationDate { get; set; }
        public long BarcodeStartNumber { get; set; }
        public long BarcodeEndNumber { get; set; }
        public string? BarcodeFormatPreview { get; set; }
        public long TotalBarcodeCount { get; set; }
        public int AllocatedCount { get; set; }
        public long Balance { get; set; }
        public int StatusId { get; set; }
        public string? Status { get; set; }
        public string? Remarks { get; set; }
        public bool IsActive { get; set; }
    }
}
