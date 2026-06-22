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

        // Assignable leaf groups for the GL-account "Account Group" picker — active leaves only,
        // optionally scoped to a company and to an account-type branch (L1 ancestor's AccountTypeId).
        Task<IReadOnlyList<AccountGroupLookupDto>> GetLeafGroupsAsync(int? companyId, int? accountTypeId);

        Task<bool> AlreadyExistsAsync(string groupCode, int? id = null);
        Task<bool> NotFoundAsync(int id);
        Task<bool> ParentExistsAsync(int parentId);
        Task<int?> GetLevelAsync(int id);

        // True when candidateId lies inside the subtree of ancestorId (circular-move guard).
        Task<bool> IsDescendantAsync(int ancestorId, int candidateId);

        Task<bool> HasChildrenAsync(int id);

        // Pending Move change requests (Status = Pending) joined to AccountGroup for the approval inbox —
        // group code/name + current/new parent names. Approver filtering is applied in the handler.
        Task<(List<AccountGroupMovePendingDto>, int)> GetMovePendingAsync(int pageNumber, int pageSize, string? searchTerm);
    }
}
