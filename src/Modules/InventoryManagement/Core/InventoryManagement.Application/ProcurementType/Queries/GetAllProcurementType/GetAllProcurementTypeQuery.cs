using Contracts.Common;
using MediatR;
using InventoryManagement.Application.ProcurementType.Dto;

namespace InventoryManagement.Application.ProcurementType.Queries.GetAllProcurementType
{
    public class GetAllProcurementTypeQuery : IRequest<ApiResponseDTO<List<ProcurementTypeDto>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SearchTerm { get; set; }
    }
}
