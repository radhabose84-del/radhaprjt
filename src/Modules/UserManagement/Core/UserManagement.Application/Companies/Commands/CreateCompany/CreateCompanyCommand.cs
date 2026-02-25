using UserManagement.Application.Companies.Queries.GetCompanies;
using MediatR;

namespace UserManagement.Application.Companies.Commands.CreateCompany
{
    public class CreateCompanyCommand : IRequest<int>
    {
        public CompanyDTO Company { get; set; } = default!;
        

    }
}