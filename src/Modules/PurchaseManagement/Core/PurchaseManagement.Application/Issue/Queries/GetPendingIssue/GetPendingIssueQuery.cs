using MediatR;

namespace PurchaseManagement.Application.Issue.Queries.GetPendingIssue
{
    public class GetPendingIssueQuery : IRequest<List<GetPendingIssueDto>>
    {
        public int MrsNo { get; set; }
    }
}