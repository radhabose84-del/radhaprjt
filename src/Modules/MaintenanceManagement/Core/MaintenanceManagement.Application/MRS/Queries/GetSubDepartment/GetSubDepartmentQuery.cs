using MediatR;

namespace MaintenanceManagement.Application.MRS.Queries.GetSubDepartment
{
    public class GetSubDepartmentQuery : IRequest<List<MSubDepartment>>
    {
        public string? OldUnitcode { get; set; }   
    }
}