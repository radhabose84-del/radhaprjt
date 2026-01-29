using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Application.UserRole.Queries.GetRolesAutocomplete
{
    public class GetUserRoleAutocompleteDto
    {
        public int Id { get; set; }
        public string? RoleName { get; set; }
    }
}