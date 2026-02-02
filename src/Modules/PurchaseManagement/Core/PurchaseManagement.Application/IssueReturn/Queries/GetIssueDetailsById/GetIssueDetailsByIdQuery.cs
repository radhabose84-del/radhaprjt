using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;

namespace PurchaseManagement.Application.IssueReturn.Queries.GetIssueDetailsById
{
    public class GetIssueDetailsByIdQuery : IRequest<List<GetIssueDetailsByIdDto>>
    {
        public int IssueHeaderId { get; set; }
        public int? ItemId { get; set; }
    }
}