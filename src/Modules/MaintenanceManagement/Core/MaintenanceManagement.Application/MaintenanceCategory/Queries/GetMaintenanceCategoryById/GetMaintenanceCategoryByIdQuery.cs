using MaintenanceManagement.Application.MaintenanceCategory.Queries.GetMaintenanceCategory;
using MediatR;

namespace MaintenanceManagement.Application.MaintenanceCategory.Queries.GetMaintenanceCategoryById
{
    public class GetMaintenanceCategoryByIdQuery : IRequest<MaintenanceCategoryDto>
    {
        public int Id { get; set; }
    }
}