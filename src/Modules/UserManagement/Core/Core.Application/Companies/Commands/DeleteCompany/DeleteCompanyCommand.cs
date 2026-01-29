using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using System.Reflection;
using System.Text;
using Core.Application.Companies.Queries.GetCompanies;
using Core.Application.Common.HttpResponse;

namespace Core.Application.Companies.Commands.DeleteCompany
{
    public class DeleteCompanyCommand : IRequest<bool>
    {
        public int Id { get; set; }
    }
}