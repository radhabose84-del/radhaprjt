namespace SalesManagement.Application.SalesReturn.Dto
{
    public class SalesReturnListDto
    {
        public int Id { get; set; }
        public string? ReturnNumber { get; set; }
        public DateOnly ReturnDate { get; set; }
        public int ComplaintHeaderId { get; set; }
        public string? ComplaintNumber { get; set; }
        public int CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public int WarehouseId { get; set; }
        public string? WarehouseName { get; set; }
        public string? StatusName { get; set; }
        public int DetailCount { get; set; }
    }
}
