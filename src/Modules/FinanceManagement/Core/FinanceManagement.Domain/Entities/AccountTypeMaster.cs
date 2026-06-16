using FinanceManagement.Domain.Common;

namespace FinanceManagement.Domain.Entities
{
    public class AccountTypeMaster : BaseEntity
    {
        public int CompanyId { get; set; }
        public string? AccountTypeName { get; set; }
        public string? StartCode { get; set; }
        public int AccountCodeLength { get; set; }
        public int SortOrder { get; set; }

        // Reverse navigation
        public ICollection<GlAccountMaster>? GlAccountMasters { get; set; }
    }
}
