using UserManagement.Domain.Common;

namespace UserManagement.Domain.Entities
{
    public class Location : BaseEntity
    {
        public int Id { get; set; }
        public string? Code { get; set; }
        public string? LocationName { get; set; }
        public string? Description { get; set; }
    }
}
