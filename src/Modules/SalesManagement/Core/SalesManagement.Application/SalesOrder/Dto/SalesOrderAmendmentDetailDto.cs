namespace SalesManagement.Application.SalesOrder.Dto
{
    public class SalesOrderAmendmentDetailDto
    {
        public int Id { get; set; }
        public int SalesOrderAmendmentHeaderId { get; set; }
        public string? ChangeType { get; set; }
        public int SalesOrderDetailId { get; set; }

        // Old Values (snapshot)
        public int OldItemId { get; set; }
        public string? OldItemName { get; set; }
        public int OldQtyInBags { get; set; }
        public decimal OldExMillRate { get; set; }
        public DateOnly OldExpectedDeliveryDate { get; set; }

        // New Values (null for Removed)
        public int? NewQtyInBags { get; set; }
        public decimal? NewExMillRate { get; set; }
        public DateOnly? NewExpectedDeliveryDate { get; set; }
    }
}
