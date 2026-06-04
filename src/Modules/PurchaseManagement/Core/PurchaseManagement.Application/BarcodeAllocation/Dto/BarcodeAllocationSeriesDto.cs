namespace PurchaseManagement.Application.BarcodeAllocation.Dto
{
    // Barcode-series dropdown row: series still having un-allocated range.
    public class BarcodeAllocationSeriesDto
    {
        public int Id { get; set; }
        public string? BarcodeSeriesNumber { get; set; }
        public long BarcodeStartNumber { get; set; }
        public long BarcodeEndNumber { get; set; }
        public int AllocatedCount { get; set; }
        public long BalanceCount { get; set; }
    }
}
