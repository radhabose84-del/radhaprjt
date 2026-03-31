namespace SalesManagement.Application.SalesOrder.Dto
{
    public class CreateSalesOrderAmendmentDetailDto
    {
        // References the existing SalesOrderDetail row being changed
        public int SalesOrderDetailId { get; set; }

        // New Values — provide at least one for "Modified"; leave all null for "Removed"
        // ChangeType is auto-derived: any New* value set → "Modified", all null → "Removed"
        public int? NewQtyInBags { get; set; }
        public decimal? NewExMillRate { get; set; }
        public DateOnly? NewExpectedDeliveryDate { get; set; }
    }
}
