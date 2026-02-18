using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Common;
using MediatR;

namespace UserManagement.Application.Companies.Commands.DeleteFileCompany
{
    public class DeleteFileCompanyCommand : IRequest<bool>
    {
        public string? Logo { get; set; }
    }
}