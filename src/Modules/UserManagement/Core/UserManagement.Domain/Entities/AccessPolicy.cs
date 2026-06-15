using UserManagement.Domain.Common;
using static UserManagement.Domain.Enums.Common.Enums;

namespace UserManagement.Domain.Entities
{
    public class AccessPolicy : BaseEntity
    {
        public AccessPolicy()
        {
            IsActive  = Status.Active;
            IsDeleted = IsDelete.NotDeleted;
        }

        public int    Id         { get; set; }
        public string PolicyCode { get; set; } = string.Empty;
        public string PolicyName { get; set; } = string.Empty;
        public string EntityName { get; set; } = string.Empty;
        public string FieldName  { get; set; } = string.Empty;

        public ICollection<RoleAccessPolicy> RoleAccessPolicies { get; set; } = new List<RoleAccessPolicy>();
    }
}
