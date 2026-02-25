using MaintenanceManagement.Application.MRS.Queries.GetDepartment;
using MediatR;

namespace MaintenanceManagement.Application.MRS.Queries
{
    public class GetDepartmentbyIdQuery : IRequest<List<MDepartmentDto>>
    {
        public string? OldUnitcode { get; set; }
    }
}