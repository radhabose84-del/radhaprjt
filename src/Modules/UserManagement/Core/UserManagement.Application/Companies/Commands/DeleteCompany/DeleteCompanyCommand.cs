using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using System.Reflection;
using System.Text;
using UserManagement.Application.Companies.Queries.GetCompanies;
using UserManagement.Application.Common.HttpResponse;

namespace UserManagement.Application.Companies.Commands.DeleteCompany
{
    public class DeleteCompanyCommand : IRequest<bool>
    {
        public int Id { get; set; }
    }
}