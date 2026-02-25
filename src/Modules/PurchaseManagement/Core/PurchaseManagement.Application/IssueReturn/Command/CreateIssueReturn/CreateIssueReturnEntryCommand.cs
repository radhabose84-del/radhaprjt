using MediatR;

namespace PurchaseManagement.Application.IssueReturn.Command.CreateIssueReturn
{
    public class CreateIssueReturnEntryCommand  : IRequest<int>
    {
        public CreateIssueReturnDto IssueReturnEntry { get; set; } = null!;
    }
}