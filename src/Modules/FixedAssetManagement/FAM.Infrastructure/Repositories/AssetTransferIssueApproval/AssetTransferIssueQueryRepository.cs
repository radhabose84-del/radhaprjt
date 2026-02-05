using System.Data;
using FAM.Application.AssetMaster.AssetTranferIssueApproval.Queries.GetAssetTransferIssueApproval;
using FAM.Application.Common.Interfaces;
using FAM.Application.Common.Interfaces.IAssetTransferIssueApproval;
using Dapper;

namespace FAM.Infrastructure.Repositories.AssetTransferIssueApproval
{
    public class AssetTransferIssueQueryRepository : IAssetTransferIssueApprovalQueryRepository
    {
        private readonly IDbConnection _dbConnection;
        private readonly IIPAddressService _ipAddressService;  

        public AssetTransferIssueQueryRepository(IDbConnection dbConnection, IIPAddressService ipAddressService)
        {
            _dbConnection = dbConnection;
            _ipAddressService = ipAddressService;
        }


    public async Task<(List<AssetTransferIssueApprovalDto>, int)> GetAllPendingAssetTransferAsync(
    int PageNumber, 
    int PageSize, 
    string? TransferType, 
    DateTimeOffset? FromDate, 
    DateTimeOffset? ToDate)
        {
            var UnitId = _ipAddressService.GetUnitId();
            var query = $$"""
                DECLARE @TotalCount INT;
                SELECT @TotalCount = COUNT(*)
                FROM FixedAsset.AssetTransferIssueHdr a
                INNER JOIN FixedAsset.MiscMaster c ON a.TransferType = c.Id
                WHERE a.Status = 'Pending' and a.FromUnitId = @UnitId
                {{(string.IsNullOrEmpty(TransferType) ? "" : "AND a.TransferType LIKE @Search")}}
                {{(FromDate.HasValue ? "AND CAST(a.DocDate AS DATE) >= CAST(@FromDate AS DATE)" : "")}}
                {{(ToDate.HasValue ? "AND CAST(a.DocDate AS DATE) <= CAST(@ToDate AS DATE)" : "")}};

                SELECT
                    a.Id,
                    a.DocDate,
                    c.Description AS TransferType,
                    a.FromUnitId,
                    CAST(NULL AS NVARCHAR(200)) AS FromUnitname,
                    a.ToUnitId,
                    CAST(NULL AS NVARCHAR(200)) AS ToUnitname,
                    a.FromDepartmentId,
                    CAST(NULL AS NVARCHAR(200)) AS FromDepartment,
                    a.ToDepartmentId,
                    CAST(NULL AS NVARCHAR(200)) AS ToDepartment,
                    a.FromCustodianId,
                    a.FromCustodianName,
                    a.ToCustodianId,
                    a.ToCustodianName,
                    a.GatePassNo,
                    a.Status
                FROM FixedAsset.AssetTransferIssueHdr a
                INNER JOIN FixedAsset.MiscMaster c ON a.TransferType = c.Id
                WHERE a.Status = 'Pending' and a.FromUnitId = @UnitId
                {{(string.IsNullOrEmpty(TransferType) ? "" : "AND a.TransferType LIKE @Search")}}
                {{(FromDate.HasValue ? "AND CAST(a.DocDate AS DATE) >= CAST(@FromDate AS DATE)" : "")}}
                {{(ToDate.HasValue ? "AND CAST(a.DocDate AS DATE) <= CAST(@ToDate AS DATE)" : "")}}
                ORDER BY a.Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                SELECT @TotalCount AS TotalCount;
            """;

            var parameters = new
            {
                Search = $"%{TransferType}%",
                FromDate,
                ToDate,
                Offset = (PageNumber - 1) * PageSize,
                PageSize,
                UnitId
            };

            var assetTransferIssue = await _dbConnection.QueryMultipleAsync(query, parameters);
            var assetTransferIssueList = (await assetTransferIssue.ReadAsync<AssetTransferIssueApprovalDto>()).ToList();
            int totalCount = await assetTransferIssue.ReadFirstAsync<int>();

            return (assetTransferIssueList, totalCount);
        }   

    public async Task<List<FAM.Domain.Entities.AssetMaster.AssetTransferIssueApproval>> GetByAssetTransferIdAsync(int Id)
    {
         var UnitId = _ipAddressService.GetUnitId();
            const string query = @"
            SELECT 
                a.Id,
                a.DocDate,
                b.AssetId,
                c.AssetCode,
                c.AssetName AS AssetName,
                b.AssetValue 
            FROM FixedAsset.AssetTransferIssueHdr a 
            INNER JOIN FixedAsset.AssetTransferIssueDtl b ON a.Id = b.AssetTransferId
            INNER JOIN FixedAsset.AssetMaster c ON b.AssetId = c.Id
            WHERE a.Status = 'Pending' AND a.Id = @Id AND a.FromUnitId = @UnitId";

        var assetTransfers = await _dbConnection.QueryAsync<FAM.Domain.Entities.AssetMaster.AssetTransferIssueApproval>(query, new { Id, UnitId });

        return assetTransfers.ToList(); // Ensure it returns a List
    }
    }
}