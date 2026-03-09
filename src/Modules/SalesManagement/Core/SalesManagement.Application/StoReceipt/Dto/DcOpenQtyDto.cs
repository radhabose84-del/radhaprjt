namespace SalesManagement.Application.StoReceipt.Dto
{
    public class DcOpenQtyDto
    {
        public int DeliveryChallanDetailId { get; set; }
        public int ItemId { get; set; }
        public string? ItemName { get; set; }
        public int LotId { get; set; }
        public string? LotCode { get; set; }
        public decimal DispatchQuantity { get; set; }
        public decimal AlreadyReceivedQuantity { get; set; }
        public decimal OpenQuantity { get; set; }
        public int UOMId { get; set; }
        public string? UOMName { get; set; }
        public int StartPackNo { get; set; }
        public int EndPackNo { get; set; }
    }
}
