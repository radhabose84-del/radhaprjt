namespace SalesManagement.Application.CustomerVisit.Dto
{
    public class CustomerVisitProductDto
    {
        public int Id { get; set; }
        public int ItemId { get; set; }
        public string? ItemName { get; set; }               // IItemLookup
        public string? ItemCode { get; set; }               // IItemLookup
    }
}
