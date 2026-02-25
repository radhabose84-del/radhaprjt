namespace PurchaseManagement.Domain.Entities.GRN.GRNEntry
{
    public class GrnPutAwayRule
    {
        public int Id { get; set; }
        public DateTimeOffset? PutAwayDate { get; set; }
        public int GrnDetailId { get; set; }
        public GrnDetail GrnHeaderPutAwayDetailsMaster { get; set; } = null!;
        public int UnitId { get; set; }
        public decimal QcAcceptedQtyPurchaseUom  { get; set; }
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
        public bool Override { get; set; }
        public int CreatedBy { get; set; }
        public DateTimeOffset? CreatedDate { get; set; }
        public string? CreatedByName { get; set; }
        public string? CreatedIP { get; set; }       

    }
}