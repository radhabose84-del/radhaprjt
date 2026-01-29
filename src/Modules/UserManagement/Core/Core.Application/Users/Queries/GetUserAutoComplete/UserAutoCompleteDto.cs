using Core.Domain.Entities;
using Core.Application.Common.Mappings;

namespace Core.Application.Users.Queries.GetUserAutoComplete
{
    public class UserAutoCompleteDto 
    {
        public int UserId { get; set; }
        public string? UserName { get; set; }
    }
}