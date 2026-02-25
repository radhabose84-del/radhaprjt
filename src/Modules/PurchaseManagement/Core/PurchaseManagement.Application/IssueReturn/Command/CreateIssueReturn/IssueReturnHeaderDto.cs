namespace PurchaseManagement.Application.IssueReturn.Command.CreateIssueReturn
{
    public class IssueReturnHeaderDto
    {
        public int Id { get; set; }
        public string? IssueReturnNo { get; set; }
        public DateTimeOffset IssueReturnDate { get; set; }
        public int UnitId { get; set; }
        public int? RequestCategoryId { get; set; }
        public int DepartmentId { get; set; }
        public string? Remarks { get; set; }
    }
}