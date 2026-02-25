namespace PurchaseManagement.Application.IssueReturn.Command.CreateIssueReturn
{
    public class IssueReturnReverseMapDto
    {
         // Represents the header information (one record)
        public IssueReturnHeaderDto? Header { get; set; }

        // Represents the detail rows (many records)
        public ICollection<IssueReturnDetailDto>? Lines { get; set; } 
    }
}