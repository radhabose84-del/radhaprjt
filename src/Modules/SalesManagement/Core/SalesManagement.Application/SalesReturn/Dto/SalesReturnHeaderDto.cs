namespace SalesManagement.Application.SalesReturn.Dto
{
    public class SalesReturnHeaderDto
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
        public int BinId { get; set; }
        public string? BinName { get; set; }
        public int StatusId { get; set; }
        public string? StatusName { get; set; }
        public string? Remarks { get; set; }

        public bool IsActive { get; set; }
        public int CreatedBy { get; set; }
        public string? CreatedByName { get; set; }
        public DateTimeOffset? CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public string? ModifiedByName { get; set; }
        public DateTimeOffset? ModifiedDate { get; set; }

        // Details
        public List<SalesReturnDetailDto>? Details { get; set; }
    }
}
