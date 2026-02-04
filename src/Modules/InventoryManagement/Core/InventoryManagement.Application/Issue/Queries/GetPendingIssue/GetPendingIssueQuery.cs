using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;

namespace InventoryManagement.Application.Issue.Queries.GetPendingIssue
{
    public class GetPendingIssueQuery : IRequest<List<GetPendingIssueDto>>
    {
         public int MrsNo { get; set; }
    }
}