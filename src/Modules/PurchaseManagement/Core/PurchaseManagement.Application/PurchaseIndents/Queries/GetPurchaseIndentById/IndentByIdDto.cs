namespace PurchaseManagement.Application.PurchaseIndents.Queries.GetPurchaseIndentById
{
    public class IndentByIdDto
    {
        public int Id { get; set; }
        public string IndentNumber { get; set; } = default!;
        public DateOnly IndentDate { get; set; }
        public int IndentTypeId { get; set; }
        public string IndentTypeName { get; set; } = default!;
        public int UnitId { get; set; }
        public string UnitName { get; set; } = default!;
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; } = default!;
        public string Purpose { get; set; } = default!;
        public ICollection<IndentDetailByIdDto> IndentDetails { get; set; } = default!;
    }
}