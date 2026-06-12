using System.Text.Json;
using static UserManagement.Domain.Enums.Common.Enums;

namespace UserManagement.Application.IconMaster.Queries.GetIconMaster
{
    public class IconMasterDto
    {
        public int Id { get; set; }
        public string? Keyword { get; set; }
        public string? IconName { get; set; }
        public string? IconLibrary { get; set; }
        public int Size { get; set; }
        public JsonElement? Style { get; set; }
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
