using Core.Domain.Common;
using static Core.Domain.Enums.Common.Enums;

namespace Core.Domain.Entities
{
    public class Cities : BaseEntity
    {
        public int Id { get; set; }
        public string? CityName   { get; set; }
        public string? CityCode { get; set; }        
        public int StateId { get; set; }
        public States? States { get; set; }
    }
}