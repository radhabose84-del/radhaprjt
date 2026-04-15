namespace SalesManagement.Application.SalesOrder.Dto
{
    public class UpdateSalesOrderDiscountDto
    {
        public int Id { get; set; }            // 0 for new rows
        public int DiscountMasterId { get; set; }
        public int SlabTypeId { get; set; }
        public int PaymentTermId { get; set; }
        public int? DiscountSlabId { get; set; }
        public decimal? DiscountValue { get; set; }
    }
}
