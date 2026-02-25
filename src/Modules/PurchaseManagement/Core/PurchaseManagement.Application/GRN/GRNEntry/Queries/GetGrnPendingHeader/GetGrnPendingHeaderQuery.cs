using Contracts.Common;
using MediatR;

namespace PurchaseManagement.Application.GRN.GRNEntry.Queries.GetGrnPendingHeader
{
    public class GetGrnPendingHeaderQuery : IRequest<ApiResponseDTO<List<GetGrnPendingHeaderDto>>>
    {
        public DateTimeOffset? FromDate { get; set; }
        public DateTimeOffset? ToDate { get; set; }
        public bool? IsGrnGenerated { get; set; }
        public bool? IsQcGenerated { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 15;
        public string? SearchTerm { get; set; }
        
    }
}