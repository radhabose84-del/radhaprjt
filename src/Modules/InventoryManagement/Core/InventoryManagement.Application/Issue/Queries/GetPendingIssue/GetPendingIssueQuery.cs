using MediatR;

namespace InventoryManagement.Application.Issue.Queries.GetPendingIssue
{
    public class GetPendingIssueQuery : IRequest<List<GetPendingIssueDto>>
    {
         public int MrsNo { get; set; }
    }
}