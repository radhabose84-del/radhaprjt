using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;

namespace PurchaseManagement.Application.IssueReturn.Queries.GetPendingIssueReturnById
{
    public class GetPendingIssueReturnByIdQuery : IRequest<PendingIssueReturnByIdDto>
    {
        public int Id { get; set; }
    }
}