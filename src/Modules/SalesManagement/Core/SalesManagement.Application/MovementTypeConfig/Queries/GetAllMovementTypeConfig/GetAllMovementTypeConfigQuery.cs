using Contracts.Common;
using MediatR;
using SalesManagement.Application.MovementTypeConfig.Dto;

namespace SalesManagement.Application.MovementTypeConfig.Queries.GetAllMovementTypeConfig
{
    public class GetAllMovementTypeConfigQuery : IRequest<ApiResponseDTO<List<MovementTypeConfigDto>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SearchTerm { get; set; }
    }
}
