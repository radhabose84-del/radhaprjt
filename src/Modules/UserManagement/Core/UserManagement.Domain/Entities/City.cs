using UserManagement.Domain.Common;

namespace UserManagement.Domain.Entities
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