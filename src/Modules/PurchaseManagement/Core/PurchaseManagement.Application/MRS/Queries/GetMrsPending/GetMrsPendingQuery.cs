using Contracts.Common;
using MediatR;

namespace PurchaseManagement.Application.MRS.Queries.GetMrsPending
{
    public class GetMrsPendingQuery : IRequest<ApiResponseDTO<List<MrsPendingDto>>>
    { 
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 15;
        public string? SearchTerm { get; set; }
    }
}