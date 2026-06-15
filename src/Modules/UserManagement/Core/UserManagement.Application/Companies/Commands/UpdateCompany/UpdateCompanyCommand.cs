using UserManagement.Application.Companies.Queries.GetCompanies;
using MediatR;
using Contracts.Common;

namespace UserManagement.Application.Companies.Commands.UpdateCompany
{
    public class UpdateCompanyCommand : IRequest<bool>, IRequirePermission
    {
        public UpdateCompanyDTO Company { get; set; } = default!;
        public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
}
