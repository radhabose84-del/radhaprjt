using MediatR;

namespace PurchaseManagement.Application.IssueReturn.Queries.GetIssueDetailsById
{
    public class GetIssueDetailsByIdQuery : IRequest<List<GetIssueDetailsByIdDto>>
    {
        public int IssueHeaderId { get; set; }
        public int? ItemId { get; set; }
    }
}