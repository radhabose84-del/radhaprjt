using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Application.Common.HttpResponse;
using MediatR;

namespace Core.Application.Companies.Commands.DeleteFileCompany
{
    public class DeleteFileCompanyCommand : IRequest<bool>
    {
        public string? Logo { get; set; }
    }
}