using System.Data;
using Contracts.Interfaces.Operations.MaintenanceManagement;
using Dapper;

namespace MaintenanceManagement.Infrastructure.Repositories.Operations;

/// <summary>
/// Applies Service-PO conversion bookkeeping to a MaintenanceRequest:
///   ConvertedToPoAmount += valueDelta (clamped to 0)
///   RequestStatusId    := Open | PartiallyConverted | FullyConverted
/// Single round-trip via a multi-statement T-SQL batch keeps the update atomic
/// without taking an explicit transaction (caller may still wrap if desired).
/// </summary>
internal sealed class MaintenanceRequestConversionRepository : IMaintenanceRequestConversionService
{
    private readonly IDbConnection _dbConnection;

    public MaintenanceRequestConversionRepository(IDbConnection dbConnection)
        => _dbConnection = dbConnection;

    public async Task<bool> ApplyServicePoConversionAsync(
        int requestId, decimal valueDelta, CancellationToken ct = default)
    {
        const string sql = @"
DECLARE @Estimated decimal(18,2) = 0;
DECLARE @Current   decimal(18,2) = 0;
DECLARE @Found     bit = 0;

SELECT @Estimated = ISNULL(EstimatedServiceCost, 0),
       @Current   = ConvertedToPoAmount,
       @Found     = 1
FROM [Maintenance].[MaintenanceRequest]
WHERE Id = @Id AND IsDeleted = 0;

IF @Found = 0
BEGIN
    SELECT CAST(0 AS int);
    RETURN;
END;

DECLARE @NewAmount decimal(18,2) =
    CASE WHEN (@Current + @ValueDelta) < 0 THEN 0
         ELSE (@Current + @ValueDelta)
    END;

DECLARE @NewStatusCode varchar(50) =
    CASE
        WHEN @NewAmount <= 0                                       THEN 'Open'
        WHEN @Estimated > 0 AND @NewAmount >= @Estimated           THEN 'FullyConverted'
        ELSE                                                            'PartiallyConverted'
    END;

DECLARE @NewStatusId int;
SELECT TOP 1 @NewStatusId = ms.Id
FROM [Maintenance].[MiscMaster] ms
INNER JOIN [Maintenance].[MiscTypeMaster] mt ON ms.MiscTypeId = mt.Id AND mt.IsDeleted = 0
WHERE mt.MiscTypeCode = 'MaintenanceStatus'
  AND ms.Code = @NewStatusCode
  AND ms.IsDeleted = 0;

IF @NewStatusId IS NULL
BEGIN
    SELECT CAST(0 AS int);
    RETURN;
END;

UPDATE [Maintenance].[MaintenanceRequest]
SET ConvertedToPoAmount = @NewAmount,
    RequestStatusId     = @NewStatusId,
    ModifiedDate        = SYSDATETIMEOFFSET()
WHERE Id = @Id;

SELECT @@ROWCOUNT;
";

        var affected = await _dbConnection.ExecuteScalarAsync<int>(
            new CommandDefinition(sql, new { Id = requestId, ValueDelta = valueDelta }, cancellationToken: ct));

        return affected > 0;
    }
}
