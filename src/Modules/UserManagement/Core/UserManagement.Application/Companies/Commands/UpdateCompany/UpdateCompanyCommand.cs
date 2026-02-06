using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserManagement.Application.Common.HttpResponse;
using UserManagement.Application.Companies.Queries.GetCompanies;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace UserManagement.Application.Companies.Commands.UpdateCompany
{
    public class UpdateCompanyCommand : IRequest<bool>
    {
        public UpdateCompanyDTO Company { get; set; }
    }
}