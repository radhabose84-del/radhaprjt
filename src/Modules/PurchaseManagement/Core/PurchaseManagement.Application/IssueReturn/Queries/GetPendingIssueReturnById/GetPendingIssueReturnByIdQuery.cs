using MediatR;

namespace PurchaseManagement.Application.IssueReturn.Queries.GetPendingIssueReturnById
{
    public class GetPendingIssueReturnByIdQuery : IRequest<PendingIssueReturnByIdDto>
    {
        public int Id { get; set; }
    }
}