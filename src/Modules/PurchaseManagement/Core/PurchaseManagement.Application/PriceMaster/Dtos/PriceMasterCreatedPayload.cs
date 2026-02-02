namespace PurchaseManagement.Application.PriceMaster.Dtos
{
    public sealed class PriceMasterCreatedPayload
    {
        public int PriceMasterId { get; set; }
        public int UnitId { get; set; }
        public int ItemId { get; set; }
        public int VendorId { get; set; }
        public DateOnly ValidFrom { get; set; }
        public DateOnly? ValidTo { get; set; }
        public int UomId { get; set; }
        public int SourceFromId { get; set; }
        public int StatusId { get; set; }

        public List<PriceMasterCreatedLine> Lines { get; set; } = new();
    }

    public sealed class PriceMasterCreatedLine
    {
        public decimal  ScaleQtyFrom { get; set; }
        public decimal? ScaleQtyTo   { get; set; }
        public decimal  UnitPrice    { get; set; }
        public int      CurrencyId   { get; set; }
    }
}
