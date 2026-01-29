using Core.Application.Common.Mappings;
using Core.Domain.Entities;
using static Core.Domain.Enums.Common.Enums;

namespace Core.Application.State.Queries.GetStates
{
    public class StateDto  : IMapFrom<States>
    {
        public int Id { get; set; }
        public string? StateCode { get; set; } 
        public string? StateName { get; set; } 
        public int CountryId { get; set; }   
         public Status IsActive { get; set; }       
        public int CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedByName { get; set; }
        public string? CreatedIP { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public string? ModifiedByName { get; set; }
        public string? ModifiedIP { get; set; }       
    }
}