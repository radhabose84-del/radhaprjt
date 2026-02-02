using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PurchaseManagement.Application.Common.HttpResponse;
using MediatR;

namespace PurchaseManagement.Application.IssueReturn.Queries.GetPendingIssueReturn
{
    public class GetPendingIssueReturnQuery : IRequest<ApiResponseDTO<List<PendingIssueReturnDto>>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 15;
        public string? SearchTerm { get; set; }
    }
}