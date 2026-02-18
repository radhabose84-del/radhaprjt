using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Common;
using UserManagement.Application.Divisions.Queries.GetDivisions;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace UserManagement.Application.Divisions.Commands.CreateDivision
{
    public class CreateDivisionCommand : IRequest<DivisionDTO>
    {
        
        public string? ShortName { get; set; }
        public string? Name { get; set; }
        public int CompanyId { get; set; }
    }
}