using UserManagement.Application.Companies.Queries.GetCompanies;
using MediatR;
using Contracts.Common;

namespace UserManagement.Application.Companies.Commands.CreateCompany
{
    public class CreateCompanyCommand : IRequest<int>, IRequirePermission
    {
        public CompanyDTO Company { get; set; } = default!;
        

        public PermissionType RequiredPermission => PermissionType.CanAdd;
    }
}
