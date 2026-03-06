using UserManagement.Domain.Common;
using static UserManagement.Domain.Enums.Common.Enums;

namespace UserManagement.Domain.Entities
{
    public class MiscMaster : BaseEntity
    {
        public int Id { get; set; }
        public int MiscTypeId { get; set; }
        public string? Code { get; set; }
        public string? Description { get; set; }
        public int SortOrder { get; set; }
        public new Status IsActive { get; set; }
        public MiscTypeMaster? MiscTypeMaster { get; set; }
        public IList<CustomField> CustomFieldDataTypes { get; set; } = new List<CustomField>();
        public IList<CustomField> CustomFieldLabelTypes { get; set; } = new List<CustomField>();
        public IList<Unit>? Units { get; set; }
        // public IList<CustomField> CustomFieldDataTypes { get; set; }
        // public IList<CustomField> CustomFieldLabelTypes { get; set; }




    }
}