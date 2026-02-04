using UserManagement.Application.Common.Mappings;
using UserManagement.Domain.Entities;
using static UserManagement.Domain.Enums.Common.Enums;

namespace UserManagement.Application.State.Queries.GetStates
{
    public class StateAutoCompleteDTO 
    {
        public int Id { get; set; }
        public string? StateCode { get; set; } 
        public string? StateName { get; set; }       
    }
}