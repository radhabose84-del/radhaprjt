using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserManagement.Application.Common.HttpResponse;
using UserManagement.Application.Divisions.Queries.GetDivisions;
using UserManagement.Application.Users.Queries.GetUsers;
using MediatR;

namespace UserManagement.Application.Divisions.Queries.GetDivisionAutoComplete
{
    public class GetDivisionAutoCompleteQuery : IRequest<List<DivisionAutoCompleteDTO>>
    {
        
        public string? SearchPattern { get; set; }
        public string? Companies { get; set; }
    }
}