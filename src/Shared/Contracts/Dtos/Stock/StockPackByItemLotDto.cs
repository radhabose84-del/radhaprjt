namespace Contracts.Dtos.Stock
{
    public class StockPackByItemLotDto
    {
        public int PackNo { get; set; }
        public int PackTypeId { get; set; }
        public int WarehouseId { get; set; }
        public int BinId { get; set; }
        public decimal NetWeight { get; set; }
        public int LotId { get; set; }
        public int ItemId { get; set; }
    }
}
