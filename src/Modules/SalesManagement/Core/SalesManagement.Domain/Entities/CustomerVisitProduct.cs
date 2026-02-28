namespace SalesManagement.Domain.Entities
{
    public class CustomerVisitProduct
    {
        public int Id { get; set; }
        public int CustomerVisitId { get; set; }            // FK → Sales.CustomerVisit
        public int ItemId { get; set; }                     // Cross-module FK → InventoryManagement (no DB constraint)

        // Navigation
        public CustomerVisit CustomerVisit { get; set; } = null!;
    }
}
