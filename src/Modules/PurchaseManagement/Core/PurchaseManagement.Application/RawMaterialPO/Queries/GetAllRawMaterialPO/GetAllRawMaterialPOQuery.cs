using Contracts.Common;
using MediatR;
using PurchaseManagement.Application.RawMaterialPO.Dto;

namespace PurchaseManagement.Application.RawMaterialPO.Queries.GetAllRawMaterialPO
{
    public class GetAllRawMaterialPOQuery : IRequest<ApiResponseDTO<List<RawMaterialPODto>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SearchTerm { get; set; }

        // PODate range filter (inclusive). null = no bound.
        public DateTimeOffset? FromDate { get; set; }
        public DateTimeOffset? ToDate { get; set; }
    }
}
