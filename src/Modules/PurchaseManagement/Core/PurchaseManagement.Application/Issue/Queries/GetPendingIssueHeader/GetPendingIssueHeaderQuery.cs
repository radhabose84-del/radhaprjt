using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PurchaseManagement.Application.Common.HttpResponse;
using MediatR;

namespace PurchaseManagement.Application.Issue.Queries.GetPendingIssueHeader
{
    public class GetPendingIssueHeaderQuery : IRequest<ApiResponseDTO<List<GetPendingIssueHeaderDto>>>
    {
        public DateTimeOffset? FromDate { get; set; }
        public DateTimeOffset? ToDate { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 15;
        public string? SearchTerm { get; set; }
    }
}