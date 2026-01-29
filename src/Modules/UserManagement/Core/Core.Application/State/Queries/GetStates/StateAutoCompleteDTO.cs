using Core.Application.Common.Mappings;
using Core.Domain.Entities;
using static Core.Domain.Enums.Common.Enums;

namespace Core.Application.State.Queries.GetStates
{
    public class StateAutoCompleteDTO 
    {
        public int Id { get; set; }
        public string? StateCode { get; set; } 
        public string? StateName { get; set; }       
    }
}