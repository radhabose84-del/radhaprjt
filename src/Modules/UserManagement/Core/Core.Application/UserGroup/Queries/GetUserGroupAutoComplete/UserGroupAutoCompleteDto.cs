using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Application.UserGroup.Queries.GetUserGroupAutoComplete
{
    public class UserGroupAutoCompleteDto
    {
         public int Id { get; set; }
        public string? GroupCode { get; set; }
        public string? GroupName { get; set; } 
    }
}