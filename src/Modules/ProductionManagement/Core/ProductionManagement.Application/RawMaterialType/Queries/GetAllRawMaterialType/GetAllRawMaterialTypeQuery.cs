using Contracts.Common;
using MediatR;
using ProductionManagement.Application.RawMaterialType.Dto;

namespace ProductionManagement.Application.RawMaterialType.Queries.GetAllRawMaterialType
{
    public class GetAllRawMaterialTypeQuery : IRequest<ApiResponseDTO<List<RawMaterialTypeDto>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SearchTerm { get; set; }
    }
}
