using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using UserManagement.Application.Common.HttpResponse;
using UserManagement.Application.Common.Mappings;
using UserManagement.Application.Companies.Queries.GetCompanies;
using UserManagement.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace UserManagement.Application.Companies.Commands.CreateCompany
{
    public class CreateCompanyCommand : IRequest<int>
    {
        public CompanyDTO Company { get; set; }
        

    }
}