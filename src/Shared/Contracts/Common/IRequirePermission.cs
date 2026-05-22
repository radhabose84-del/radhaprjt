namespace Contracts.Common;

public interface IRequirePermission
{
    PermissionType RequiredPermission { get; }
}
