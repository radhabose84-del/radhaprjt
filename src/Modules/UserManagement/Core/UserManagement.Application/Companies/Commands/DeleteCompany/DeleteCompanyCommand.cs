using MediatR;
using Contracts.Common;

namespace UserManagement.Application.Companies.Commands.DeleteCompany
{
    public class DeleteCompanyCommand : IRequest<bool>, IRequirePermission
    {
        public int Id { get; set; }
        public PermissionType RequiredPermission => PermissionType.CanDelete;
    }
}
