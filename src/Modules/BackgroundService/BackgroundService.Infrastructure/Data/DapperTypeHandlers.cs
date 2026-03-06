#nullable disable
using System.Data;
using Dapper;
using static BackgroundService.Domain.Common.BaseEntity;

namespace BackgroundService.Infrastructure.Data;

/// <summary>
/// Teaches Dapper how to map SQL bit (returned as bool by SqlClient) to the
/// BaseEntity.Status and BaseEntity.IsDelete enums used on all BackgroundService entities.
/// Registered once in DependencyInjection.AddInfrastructureServices().
/// </summary>
internal sealed class StatusTypeHandler : SqlMapper.TypeHandler<Status>
{
    public override Status Parse(object value) => value switch
    {
        bool b => b ? Status.Active : Status.Inactive,
        int  i => (Status)i,
        _      => Status.Active
    };

    public override void SetValue(IDbDataParameter parameter, Status value)
        => parameter.Value = value == Status.Active;
}

internal sealed class IsDeleteTypeHandler : SqlMapper.TypeHandler<IsDelete>
{
    public override IsDelete Parse(object value) => value switch
    {
        bool b => b ? IsDelete.Deleted : IsDelete.NotDeleted,
        int  i => (IsDelete)i,
        _      => IsDelete.NotDeleted
    };

    public override void SetValue(IDbDataParameter parameter, IsDelete value)
        => parameter.Value = value == IsDelete.Deleted;
}
