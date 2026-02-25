using Contracts.Common;
using MediatR;

namespace MaintenanceManagement.Application.MaintenanceType.Queries.GetMaintenanceType
{
    public class GetMaintenanceTypeQuery : IRequest<ApiResponseDTO<List<MaintenanceTypeDto>>>
    {
        
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 15;
        public string? SearchTerm { get; set; }
    }
}