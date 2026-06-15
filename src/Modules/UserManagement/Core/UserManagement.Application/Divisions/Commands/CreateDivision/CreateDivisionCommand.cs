using UserManagement.Application.Divisions.Queries.GetDivisions;
using MediatR;
using Contracts.Common;

namespace UserManagement.Application.Divisions.Commands.CreateDivision
{
    public class CreateDivisionCommand : IRequest<DivisionDTO>, IRequirePermission
    {
        
        public string? ShortName { get; set; }
        public string? Name { get; set; }
        public int CompanyId { get; set; }
        public PermissionType RequiredPermission => PermissionType.CanAdd;
    }
}
