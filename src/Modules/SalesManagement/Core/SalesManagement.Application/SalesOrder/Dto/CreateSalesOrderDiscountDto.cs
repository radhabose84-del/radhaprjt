namespace SalesManagement.Application.SalesOrder.Dto
{
    public class CreateSalesOrderDiscountDto
    {
        public int DiscountMasterId { get; set; }
        public int SlabTypeId { get; set; }
        public int PaymentTermId { get; set; }
    }
}
