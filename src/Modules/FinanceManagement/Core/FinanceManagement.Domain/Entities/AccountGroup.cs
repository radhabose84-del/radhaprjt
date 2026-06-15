using FinanceManagement.Domain.Common;

namespace FinanceManagement.Domain.Entities
{
    public class AccountGroup : BaseEntity
    {
        // Textile default hierarchy depth (Segment → Group → Sub-group → Account).
        // Accounts attach only at the leaf level (Level == DefaultMaxDepth).
        public const int DefaultMaxDepth = 4;

        // Interim Level 1 whitelist. TODO: replace with the AccountTypeMaster reference
        // table when that feature is built; validators currently read this list.
        public static readonly IReadOnlyCollection<string> Level1GroupNames = new[]
        {
            "Assets", "Liabilities", "Equity", "Revenue", "Expenses"
        };

        public int CompanyId { get; set; }

        public string GroupCode { get; set; } = null!;
        public string GroupName { get; set; } = null!;

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
    }
}
