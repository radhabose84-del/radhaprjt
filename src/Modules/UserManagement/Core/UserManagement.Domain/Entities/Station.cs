using UserManagement.Domain.Common;

namespace UserManagement.Domain.Entities
{
    public class Station : BaseEntity
    {
        public int Id { get; set; }
        public string? Code { get; set; }
        public string? StationName { get; set; }
        public string? Description { get; set; }
    }
}
