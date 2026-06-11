namespace PurchaseManagement.Application.Arrival.Dto
{
    /// <summary>
    /// Remaining (balance) quantity per item for a Raw Material PO, scoped to the current unit.
    /// BalanceQty = OrderedQty (RawMaterialPODetail.Quantity) − ArrivedQty (Σ ArrivalDetail.ArrivedQty).
    /// </summary>
    public sealed class ArrivalBalanceQtyDto
    {
        public int ItemId { get; set; }
        public string? ItemName { get; set; }
        public decimal OrderedQty { get; set; }
        public decimal ArrivedQty { get; set; }
        public decimal BalanceQty { get; set; }
    }
}
