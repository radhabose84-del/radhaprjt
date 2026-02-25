namespace PurchaseManagement.Application.PurchaseIndents.Queries.GetAllPurchaseIndent
{
    public class IndentDto
    {
        public int Id { get; set; }
        public string? IndentNumber { get; set; }
        public DateOnly IndentDate { get; set; }
        public int IndentTypeId { get; set; }
        public string? IndentType { get; set; }
        public int UnitId { get; set; }
        public string? UnitName { get; set; }
        public int DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public string? Purpose { get; set; }
        public string? Status { get; set; }
        public byte IsActive { get; set; }
        public int CreatedBy { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public string? CreatedByName { get; set; }
        public int ModifiedBy { get; set; }
        public DateTimeOffset ModifiedDate { get; set; }
        public string? ModifiedByName { get; set; }
        public int ItemCount { get; set; }
        public string? POStatus { get; set; }
    }
}