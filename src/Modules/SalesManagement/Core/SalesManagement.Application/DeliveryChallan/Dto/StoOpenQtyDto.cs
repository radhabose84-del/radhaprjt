namespace SalesManagement.Application.DeliveryChallan.Dto
{
    public class StoOpenQtyDto
    {
        public int StoDetailId { get; set; }
        public int ItemId { get; set; }
        public string? ItemName { get; set; }
        public decimal OrderedQty { get; set; }
        public decimal DispatchedQty { get; set; }
        public decimal OpenQty { get; set; }
        public int UOMId { get; set; }
        public string? UOMName { get; set; }
        public decimal TransferPrice { get; set; }
    }
}
