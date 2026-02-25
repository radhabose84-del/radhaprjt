using Contracts.Common;
using MediatR;

namespace MaintenanceManagement.Application.MaintenanceCategory.Queries.GetMaintenanceCategory
{
    public class GetMaintenanceCategoryQuery : IRequest<ApiResponseDTO<List<MaintenanceCategoryDto>>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 15;
        public string? SearchTerm { get; set; }
    }
}