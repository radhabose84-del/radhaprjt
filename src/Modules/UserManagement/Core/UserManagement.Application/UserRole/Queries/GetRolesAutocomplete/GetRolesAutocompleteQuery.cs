using Contracts.Common;
using UserManagement.Application.UserRole.Queries.GetRole;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserManagement.Application.UserRole.Queries.GetRolesAutocomplete
{
    public class GetRolesAutocompleteQuery : IRequest<List<GetUserRoleAutocompleteDto>>
    {
        public string? SearchTerm { get; set; }
    }
}