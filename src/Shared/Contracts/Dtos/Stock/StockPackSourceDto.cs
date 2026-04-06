namespace Contracts.Dtos.Stock
{
    public class StockPackSourceDto
    {
        public int LotId { get; set; }
        public int WarehouseId { get; set; }
        public int BinId { get; set; }
        public decimal OldNetWeightPerPack { get; set; }
        public int OldTotalBags { get; set; }
        public decimal OldNetWeight { get; set; }
    }
}
