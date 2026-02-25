using UserManagement.Application.Companies.Queries.GetCompanies;
using MediatR;

namespace UserManagement.Application.Companies.Commands.UpdateCompany
{
    public class UpdateCompanyCommand : IRequest<bool>
    {
        public UpdateCompanyDTO Company { get; set; } = default!;
    }
}