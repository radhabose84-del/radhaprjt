using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserManagement.Application.Common.HttpResponse;
using MediatR;

namespace UserManagement.Application.Companies.Commands.DeleteFileCompany
{
    public class DeleteFileCompanyCommand : IRequest<bool>
    {
        public string? Logo { get; set; }
    }
}