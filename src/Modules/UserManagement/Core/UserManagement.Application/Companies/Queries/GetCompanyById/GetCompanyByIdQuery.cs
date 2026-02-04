using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using System.Text;
using UserManagement.Application.Companies.Queries.GetCompanies;
using UserManagement.Application.Common.HttpResponse;

namespace UserManagement.Application.Companies.Queries.GetCompanyById
{
    public class GetCompanyByIdQuery : IRequest<GetByIdDTO>
    {
        public int CompanyId { get; set; }
    }
}