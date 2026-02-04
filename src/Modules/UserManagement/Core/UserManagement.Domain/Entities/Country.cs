using UserManagement.Domain.Common;

namespace UserManagement.Domain.Entities
{
    public class Countries  : BaseEntity
    {
        public int Id { get; set; }
        public string? CountryCode { get; set; }
        public string? CountryName { get; set; }
        public ICollection<States> States { get; set; } = new List<States>();
    }
}