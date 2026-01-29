using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Application.Common.HttpResponse;
using Core.Application.Companies.Queries.GetCompanies;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Core.Application.Companies.Commands.UploadFileCompany
{
    public class UploadFileCompanyCommand : IRequest<GetCompanyDTO>
    {
        public IFormFile? File { get; set; }
    }
}