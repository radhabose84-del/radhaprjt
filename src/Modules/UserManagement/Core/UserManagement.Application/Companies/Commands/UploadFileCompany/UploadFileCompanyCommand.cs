using UserManagement.Application.Companies.Queries.GetCompanies;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace UserManagement.Application.Companies.Commands.UploadFileCompany
{
    public class UploadFileCompanyCommand : IRequest<GetCompanyDTO>
    {
        public IFormFile? File { get; set; }
    }
}