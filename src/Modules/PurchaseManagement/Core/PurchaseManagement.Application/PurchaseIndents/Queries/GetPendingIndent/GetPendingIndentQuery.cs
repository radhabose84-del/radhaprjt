using Contracts.Common;
using MediatR;

namespace PurchaseManagement.Application.PurchaseIndents.Queries.GetPendingIndent
{
    public class GetPendingIndentQuery : IRequest<ApiResponseDTO<List<PendingIndentDto>>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 15;
        public string? SearchTerm { get; set; }
    }
}