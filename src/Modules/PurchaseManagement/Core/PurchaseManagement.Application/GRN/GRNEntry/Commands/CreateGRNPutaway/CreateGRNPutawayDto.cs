namespace PurchaseManagement.Application.GRN.GRNEntry.Commands.CreateGRNPutaway
{
    public class CreateGRNPutawayDto
    {
        public int GrnDetailId { get; set; }
        public int UnitId { get; set; }
        public decimal QcAcceptedQtyPurchaseUom { get; set; }
        public int GrnId { get; set; }
        public int PoId { get; set; }
        public int PoSlNoLocal { get; set; }
        public int ItemId { get; set; }
        public int WarehouseId { get; set; }
        public int StorageTypeId { get; set; }
        public int TargetId { get; set; }
        public int PriorityId { get; set; }
        public int PurchaseUomId { get; set; }
        public int StockUomId { get; set; }
        public decimal? ConversionFactor { get; set; }
        public decimal QcAcceptedQtyStockUom { get; set; }
        public byte Override { get; set; }  
        
       
    }
}