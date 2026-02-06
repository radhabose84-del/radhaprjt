using UserManagement.Application.Common.Mappings;
using UserManagement.Domain.Entities;
using static UserManagement.Domain.Enums.Common.Enums;

namespace UserManagement.Application.City.Queries.GetCities
{
    public class CityDto  : IMapFrom<Cities>
    {
        public int Id { get; set; }
        public string? CityCode { get; set; }
        public string? CityName { get; set; } 
        public int StateId { get; set; }
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