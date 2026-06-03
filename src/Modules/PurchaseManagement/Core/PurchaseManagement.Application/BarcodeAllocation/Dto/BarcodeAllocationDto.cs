namespace PurchaseManagement.Application.BarcodeAllocation.Dto
{
    public class BarcodeAllocationDto
    {
        public int Id { get; set; }
        public string? AllocationNumber { get; set; }
        public DateTimeOffset AllocationDate { get; set; }
        public string? EmployeeNo { get; set; }
        public string? EmployeeName { get; set; }
        public int BarcodeSeriesId { get; set; }
        public string? BarcodeSeriesNumber { get; set; }
        public string? Prefix { get; set; }
        public long BarcodeFrom { get; set; }
        public long BarcodeTo { get; set; }
        public long TotalAllocatedQuantity { get; set; }
        public int UsedQuantity { get; set; }
        public long BalanceQuantity { get; set; }
        public int StatusId { get; set; }
        public string? Status { get; set; }
        public string? Remarks { get; set; }
        public bool IsActive { get; set; }
    }
}
