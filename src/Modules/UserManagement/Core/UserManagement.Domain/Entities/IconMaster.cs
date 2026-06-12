using UserManagement.Domain.Common;

namespace UserManagement.Domain.Entities
{
    public class IconMaster : BaseEntity
    {
        public int Id { get; set; }
        public string? Keyword { get; set; }
        public string? IconName { get; set; }
        public string? IconLibrary { get; set; }
        public int Size { get; set; } = 18;
        public string? Style { get; set; }
    }
}
