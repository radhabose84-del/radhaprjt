using FinanceManagement.Domain.Common;

namespace FinanceManagement.Domain.Entities
{
    public class AccountGroup : BaseEntity
    {
        // Maximum hierarchy depth. Groups may nest up to 6 levels; creating a child below
        // a Level 6 node is blocked. Accounts attach at the leaf (a node with no children).
        public const int DefaultMaxDepth = 6;

        public int CompanyId { get; set; }

        public string GroupCode { get; set; } = null!;
        public string GroupName { get; set; } = null!;

        // Statutory head — set only on Level 1 groups (NULL below). FK to Finance.AccountTypeMaster.
        public int? AccountTypeId { get; set; }
        public AccountTypeMaster? AccountType { get; set; }

        // Self-referencing parent (NULL = Level 1 root). Enforces the single-parent rule structurally.
        public int? ParentAccountGroupId { get; set; }
        public int Level { get; set; }

        // Maintained on create/move/delete — a node is a leaf when it has no children.
        // Accounts attach only to leaf groups.
        public bool IsLeaf { get; set; }

        // Display ordering among siblings.
        public int SortOrder { get; set; }

        // Same-module self navigation
        public AccountGroup? ParentAccountGroup { get; set; }
        public ICollection<AccountGroup>? Children { get; set; }

        // Reverse navigation
        public ICollection<GlAccountMaster>? GlAccountMasters { get; set; }
    }
}
