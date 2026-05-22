using Contracts.Common;

namespace Contracts.Interfaces;

public interface IPermissionService
{
    Task<bool> HasPermissionAsync(int userId, int menuId, PermissionType permission, CancellationToken ct = default);
    void InvalidateCache(int userId);
}
