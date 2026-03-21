namespace SalesManagement.Domain.Entities
{
    public class DispatchAdviceDetail
    {
        public int Id { get; set; }
        public int DispatchAdviceHeaderId { get; set; }     // FK → Sales.DispatchAdviceHeader
        public int SalesOrderDetailId { get; set; }         // FK → Sales.SalesOrderDetail
        public int ItemId { get; set; }                     // Cross-module FK → InventoryManagement
        public int LotId { get; set; }                      // Cross-module FK → ProductionManagement
        public int StartPackNo { get; set; }
        public int EndPackNo { get; set; }
        public decimal DispatchQty { get; set; }
        public int PackTypeId { get; set; }                    // Cross-module FK → ProductionManagement

        // Navigation properties
        public DispatchAdviceHeader? DispatchAdviceHeader { get; set; }
        public SalesOrderDetail? SalesOrderDetail { get; set; }
        // LotMaster and PackType moved to ProductionManagement — use lookups for names
    }
}
