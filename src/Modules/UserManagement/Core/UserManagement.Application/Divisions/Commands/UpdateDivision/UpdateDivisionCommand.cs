using MediatR;
using Contracts.Common;

namespace UserManagement.Application.Divisions.Commands.UpdateDivision
{
    public class UpdateDivisionCommand : IRequest<bool>, IRequirePermission
    {
         public int Id { get; set; }
        public string ShortName { get; set; } = default!;
        public string Name { get; set; } = default!;
        public int CompanyId { get; set; }
        public byte IsActive { get; set; }
         public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
}
