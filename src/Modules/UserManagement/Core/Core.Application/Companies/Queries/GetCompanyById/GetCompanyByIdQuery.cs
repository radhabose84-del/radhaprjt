using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using System.Text;
using Core.Application.Companies.Queries.GetCompanies;
using Core.Application.Common.HttpResponse;

namespace Core.Application.Companies.Queries.GetCompanyById
{
    public class GetCompanyByIdQuery : IRequest<GetByIdDTO>
    {
        public int CompanyId { get; set; }
    }
}