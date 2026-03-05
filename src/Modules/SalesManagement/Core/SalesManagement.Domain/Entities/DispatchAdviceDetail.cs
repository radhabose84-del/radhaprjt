namespace SalesManagement.Domain.Entities
{
    public class DispatchAdviceDetail
    {
        public int Id { get; set; }
        public int DispatchAdviceHeaderId { get; set; }     // FK → Sales.DispatchAdviceHeader
        public int SalesOrderDetailId { get; set; }         // FK → Sales.SalesOrderDetail
        public int ItemId { get; set; }                     // Cross-module FK → InventoryManagement
        public int LotId { get; set; }                      // FK → Sales.LotMaster
        public int StartPackNo { get; set; }
        public int EndPackNo { get; set; }
        public decimal DispatchQty { get; set; }

        // Navigation properties
        public DispatchAdviceHeader? DispatchAdviceHeader { get; set; }
        public SalesOrderDetail? SalesOrderDetail { get; set; }
        public LotMaster? LotMaster { get; set; }
    }
}
