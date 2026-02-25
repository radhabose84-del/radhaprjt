using MediatR;
using UserManagement.Application.Companies.Queries.GetCompanies;

namespace UserManagement.Application.Companies.Queries.GetCompanyById
{
    public class GetCompanyByIdQuery : IRequest<GetByIdDTO>
    {
        public int CompanyId { get; set; }
    }
}