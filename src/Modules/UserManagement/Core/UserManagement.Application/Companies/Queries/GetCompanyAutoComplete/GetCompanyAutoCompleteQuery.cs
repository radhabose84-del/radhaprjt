using UserManagement.Application.Companies.Queries.GetCompanies;
using MediatR;

namespace UserManagement.Application.Companies.Queries.GetCompanyAutoComplete
{
    public class GetCompanyAutoCompleteQuery : IRequest<List<CompanyAutoCompleteDTO>>
    {
        
        public string? SearchPattern { get; set; }
    }
}