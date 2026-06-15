using FinanceManagement.Application.AccountGroup.Dto;

namespace FinanceManagement.Application.Common.Interfaces.IAccountGroup
{
    public interface IAccountGroupQueryRepository
    {
        // Full hierarchy (roots with nested Children) — the tree grid.
        // Optionally scoped to a company.
        Task<List<AccountGroupTreeDto>> GetTreeAsync(int? companyId);

        Task<AccountGroupDetailDto?> GetByIdAsync(int id);

        Task<IReadOnlyList<AccountGroupLookupDto>> AutocompleteAsync(string term, CancellationToken ct);

        // Eligible parents for the Move modal — active groups at the requested level,
        // optionally scoped to a company.
        Task<IReadOnlyList<AccountGroupLookupDto>> GetParentsByLevelAsync(int level, int? companyId);

        Task<bool> AlreadyExistsAsync(string groupCode, int? id = null);
        Task<bool> NotFoundAsync(int id);
        Task<bool> ParentExistsAsync(int parentId);
        Task<int?> GetLevelAsync(int id);

        // True when candidateId lies inside the subtree of ancestorId (circular-move guard).
        Task<bool> IsDescendantAsync(int ancestorId, int candidateId);

        Task<bool> HasChildrenAsync(int id);
    }
}
