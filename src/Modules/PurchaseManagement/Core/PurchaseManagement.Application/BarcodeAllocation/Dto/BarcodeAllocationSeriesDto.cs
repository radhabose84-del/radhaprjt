namespace PurchaseManagement.Application.BarcodeAllocation.Dto
{
    // Barcode-series dropdown row: series still having un-allocated range.
    // Carries the full allocatable picture so the allocation form needs only this one call:
    // range (Start..End), Total, AllocatedCount (given out), BalanceCount (left), NextFrom (start the next allocation here).
    public class BarcodeAllocationSeriesDto
    {
        public int Id { get; set; }
        public string? BarcodeSeriesNumber { get; set; }
        public long BarcodeStartNumber { get; set; }
        public long BarcodeEndNumber { get; set; }
        public long TotalBarcodeCount { get; set; }
        public int AllocatedCount { get; set; }
        public long BalanceCount { get; set; }
        public long NextFrom { get; set; }
    }
}
