using UserManagement.Domain.Entities;
using UserManagement.Application.Common.Mappings;

namespace UserManagement.Application.Users.Queries.GetUserAutoComplete
{
    public class UserAutoCompleteDto 
    {
        public int UserId { get; set; }
        public string? UserName { get; set; }
    }
}