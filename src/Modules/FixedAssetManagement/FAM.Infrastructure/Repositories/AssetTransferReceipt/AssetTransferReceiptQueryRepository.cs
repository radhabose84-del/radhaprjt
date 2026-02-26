using System.Data;
using System.Text.Json;
using FAM.Application.AssetMaster.AssetTransferIssue.Queries.GetAssetTransfered;
using FAM.Application.AssetMaster.AssetTransferReceipt.Queries.GetAssetReceiptDetails;
using FAM.Application.AssetMaster.AssetTransferReceipt.Queries.GetAssetReceiptDetailsById;
using FAM.Application.AssetMaster.AssetTransferReceipt.Queries.GetAssetReceiptPending;
using FAM.Application.AssetMaster.AssetTransferReceipt.Queries.GetAssetRecieptDtlPending;
using FAM.Application.Common.Interfaces;
using FAM.Application.Common.Interfaces.IAssetTransferReceipt;
using Dapper;

namespace FAM.Infrastructure.Repositories.AssetTransferReceipt
{
    public class AssetTransferReceiptQueryRepository : IAssetTransferReceiptQueryRepository
    {
        private readonly IDbConnection _dbConnection;
        private readonly IIPAddressService _ipAddressService;  

        public AssetTransferReceiptQueryRepository(IDbConnection dbConnection, IIPAddressService ipAddressService)
        {
            _dbConnection = dbConnection;
            _ipAddressService = ipAddressService;
        }

         public async Task<(List<AssetReceiptDetailsDto>, int)> GetAllAssetReceiptDetails(int PageNumber, int PageSize, string Receiptno, DateTimeOffset? FromDate, DateTimeOffset? ToDate)
        {
            var UnitId = _ipAddressService.GetUnitId();
             var query = $$"""
                DECLARE @TotalCount INT;
                SELECT @TotalCount = COUNT(*)
                FROM FixedAsset.AssetTransferReceiptHdr A
                INNER JOIN FixedAsset.AssetTransferIssueHdr B ON A.AssetTransferId = B.Id
                INNER JOIN FixedAsset.MiscMaster C ON B.TransferType = C.Id
                WHERE B.ToUnitId = @UnitId
                {{(string.IsNullOrEmpty(Receiptno) ? "" : "AND A.Id LIKE @Search")}}
                {{(FromDate.HasValue ? "AND A.DocDate >= @FromDate" : "")}}
                {{(ToDate.HasValue ? "AND A.DocDate <= @ToDate" : "")}};

                SELECT
                A.Id AS AssetReceiptId,
                A.AssetTransferId,
                A.DocDate,
                C.Description AS TransferType,
                B.FromUnitId,
                CAST(NULL AS NVARCHAR(200)) AS FromUnitname,
                B.ToUnitId,
                CAST(NULL AS NVARCHAR(200)) AS ToUnitname,
                B.FromDepartmentId,
                CAST(NULL AS NVARCHAR(200)) AS FromDepartment,
                B.ToDepartmentId,
                CAST(NULL AS NVARCHAR(200)) AS ToDepartment,
                B.FromCustodianId,
                B.FromCustodianName,
                B.ToCustodianId,
                B.ToCustodianName,
                A.Sdcno,
                B.GatePassNo,
                A.Remarks,
                A.AuthorizedByName,
                A.AuthorizedDate
                FROM FixedAsset.AssetTransferReceiptHdr A
                INNER JOIN FixedAsset.AssetTransferIssueHdr B ON A.AssetTransferId = B.Id
                INNER JOIN FixedAsset.MiscMaster C ON B.TransferType = C.Id
                WHERE B.ToUnitId = @UnitId
                {{(string.IsNullOrEmpty(Receiptno) ? "" : "AND A.Id LIKE @Search")}}
                {{(FromDate.HasValue ? "AND A.DocDate >= @FromDate" : "")}}
                {{(ToDate.HasValue ? "AND A.DocDate <= @ToDate" : "")}}
                ORDER BY a.Id ASC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                SELECT @TotalCount AS TotalCount;
            """;

            var parameters = new
            {
                Search = $"%{Receiptno}%",
                FromDate,
                ToDate,
                Offset = (PageNumber - 1) * PageSize,
                PageSize,
                UnitId
            };

            var assetTransferreceipt = await _dbConnection.QueryMultipleAsync(query, parameters);
            var assetTransferreceiptList = (await assetTransferreceipt.ReadAsync<AssetReceiptDetailsDto>()).ToList();
            int totalCount = await assetTransferreceipt.ReadFirstAsync<int>();

            return (assetTransferreceiptList, totalCount);
        }
         public async Task<List<AssetReceiptDetailsByIdDto>> GetByAssetReceiptId(int AssetReceiptId)
        {
            
             const string query = @"
            SELECT 
            b.AssetReceiptId,
            a.AssetTransferId,
            b.AssetId,
            c.AssetCode,
            c.AssetName,
            d.LocationName,
            e.SubLocationName,
            b.UserID,
            b.UserName 
            from
            FixedAsset.AssetTransferReceiptHdr a
            INNER JOIN FixedAsset.AssetTransferReceiptDtl b on a.Id=b.AssetReceiptId
            INNER JOIN FixedAsset.AssetMaster c on b.AssetId=c.Id
            INNER JOIN FixedAsset.Location d on b.LocationId=d.Id
            INNER JOIN FixedAsset.SubLocation e on b.SubLocationId=e.Id
	        WHERE a.Id= @AssetReceiptId and b.AckStatus=1";

            var assetreceiptList = await _dbConnection.QueryAsync<AssetReceiptDetailsByIdDto>(query, new { AssetReceiptId });

            return assetreceiptList.ToList(); // Ensure it returns a List
        }

        public async Task<(List<AssetTransferReceiptPendingDto>, int)> GetAllPendingAssetTransferAsync(
            int PageNumber, int PageSize, int? AssetTransferId, string TransferType, DateTimeOffset? FromDate, DateTimeOffset? ToDate)
        {
            var UnitId = _ipAddressService.GetUnitId();
            var query = $$"""
                DECLARE @TotalCount INT;
                SELECT @TotalCount = COUNT(DISTINCT A.Id)
                FROM FixedAsset.AssetTransferIssueHdr A
                INNER JOIN FixedAsset.AssetTransferIssueDtl B ON A.Id = B.AssetTransferId
                INNER JOIN FixedAsset.AssetMaster M ON B.AssetId = M.Id
                INNER JOIN FixedAsset.MiscMaster C ON A.TransferType = C.Id
                LEFT JOIN FixedAsset.AssetTransferReceiptHdr RH ON A.Id = RH.AssetTransferId
                LEFT JOIN FixedAsset.AssetTransferReceiptDtl RD ON RH.Id = RD.AssetReceiptId AND B.AssetId = RD.AssetId
                WHERE  A.ToUnitId = @UnitId
                AND (RD.AckStatus = 0 OR RD.AckStatus IS NULL)
                {{(AssetTransferId.HasValue ? "AND A.Id = @AssetTransferId" : "")}}
                {{(string.IsNullOrEmpty(TransferType) ? "" : "AND A.TransferType LIKE @Search")}}
                {{(FromDate.HasValue ? "AND CAST(A.DocDate AS DATE) >= CAST(@FromDate AS DATE)" : "")}}
                {{(ToDate.HasValue ? "AND CAST(A.DocDate AS DATE) <= CAST(@ToDate AS DATE)" : "")}};

                SELECT
                    Distinct(A.Id) AS AssetTransferId,
                    A.DocDate,
                    C.Description AS TransferType,
                    A.FromUnitId,
                    CAST(NULL AS NVARCHAR(200)) AS FromUnitname,
                    A.ToUnitId,
                    CAST(NULL AS NVARCHAR(200)) AS ToUnitname,
                    A.FromDepartmentId,
                    CAST(NULL AS NVARCHAR(200)) AS FromDepartment,
                    A.ToDepartmentId,
                    CAST(NULL AS NVARCHAR(200)) AS ToDepartment,
                    A.FromCustodianName,
                    A.ToCustodianName,
                    A.Status,
                    RH.Sdcno,
                    A.GatePassNo
                FROM FixedAsset.AssetTransferIssueHdr A
                INNER JOIN FixedAsset.AssetTransferIssueDtl B ON A.Id = B.AssetTransferId
                INNER JOIN FixedAsset.AssetMaster M ON B.AssetId = M.Id
                INNER JOIN FixedAsset.MiscMaster C ON A.TransferType = C.Id
                LEFT JOIN FixedAsset.AssetTransferReceiptHdr RH ON A.Id = RH.AssetTransferId
                LEFT JOIN FixedAsset.AssetTransferReceiptDtl RD ON RH.Id = RD.AssetReceiptId AND B.AssetId = RD.AssetId
                WHERE  A.ToUnitId = @UnitId
                AND (RD.AckStatus = 0 OR RD.AckStatus IS NULL)
                {{(AssetTransferId.HasValue ? "AND A.Id = @AssetTransferId" : "")}}
                {{(string.IsNullOrEmpty(TransferType) ? "" : "AND A.TransferType LIKE @Search")}}
                {{(FromDate.HasValue ? "AND CAST(A.DocDate AS DATE) >= CAST(@FromDate AS DATE)" : "")}}
                {{(ToDate.HasValue ? "AND CAST(A.DocDate AS DATE) <= CAST(@ToDate AS DATE)" : "")}}
                ORDER BY A.Id ASC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                SELECT @TotalCount AS TotalCount;
            """;

            var parameters = new
            {
                AssetTransferId,
                Search = $"%{TransferType}%",
                FromDate,
                ToDate,
                Offset = (PageNumber - 1) * PageSize,
                PageSize,
                UnitId
            };

            var assetTransferIssue = await _dbConnection.QueryMultipleAsync(query, parameters);
            var assetTransferIssueList = (await assetTransferIssue.ReadAsync<AssetTransferReceiptPendingDto>()).ToList();
            int totalCount = await assetTransferIssue.ReadFirstAsync<int>();

            return (assetTransferIssueList, totalCount);
        }

    //     public async Task<AssetTransferJsonDto> GetAssetTransferByIdAsync(int assetTransferId)
    //     {
        // const string query = @"
        //     SELECT Id as AssetTransferId , DocDate, TransferType, FromUnitId, ToUnitId, FromDepartmentId, ToDepartmentId, 
        //            FromCustodianId, ToCustodianId, Status, FromCustodianName, ToCustodianName
        //     FROM FixedAsset.AssetTransferIssueHdr
        //     WHERE Id = @AssetTransferId AND Status = 'Approved' 
        //     FOR JSON PATH, INCLUDE_NULL_VALUES;

        //     SELECT AssetId, AssetValue 
        //     FROM FixedAsset.AssetTransferIssueDtl
        //     WHERE AssetTransferId = @AssetTransferId
        //     FOR JSON PATH, INCLUDE_NULL_VALUES;
        // ";

        // using var multiQuery = await _dbConnection.QueryMultipleAsync(query, new { assetTransferId });

        // string headerJson = await multiQuery.ReadFirstOrDefaultAsync<string>();
        // string detailsJson = await multiQuery.ReadFirstOrDefaultAsync<string>();

        // if (string.IsNullOrWhiteSpace(headerJson))
        // {
        //     return null;
        // }

        // var header = JsonSerializer.Deserialize<List<AssetTransferJsonDto>>(headerJson, new JsonSerializerOptions
        // {
        //     PropertyNameCaseInsensitive = true
        // })?.FirstOrDefault();

        // var details = JsonSerializer.Deserialize<List<AssetTransferDetailJsonDto>>(detailsJson ?? "[]", new JsonSerializerOptions
        // {
        //     PropertyNameCaseInsensitive = true
        // });

        // if (header != null)
        // {
        //     header.AssetTransferDetails = details ?? new List<AssetTransferDetailJsonDto>();
        // }

        // return header;
    // }

        public async Task<AssetTransferDto> GetByAssetTransferId(int assetTransferId)
        {
            var UnitId = _ipAddressService.GetUnitId();
             const string query = @"
                SELECT ToUnitId, ToDepartmentId, ToCustodianId
                FROM FixedAsset.AssetTransferIssueHdr
                WHERE Id = @AssetTransferId AND ToUnitId = @UnitId";

                var parameters = new { AssetTransferId = assetTransferId, UnitId };

                var assetTransfer = await _dbConnection.QueryFirstOrDefaultAsync<AssetTransferDto>(query, parameters);

            return assetTransfer;
        }

        public async Task<AssetTrasnferReceiptHdrPendingDto> GetAssetTransferByIdAsync(int assetTransferId)
        {
            var UnitId = _ipAddressService.GetUnitId();
             const string query = @"
                    SELECT
                    Distinct(A.Id) AS AssetTransferId,
                    A.DocDate,
                    C.Description AS TransferType,
                    A.FromUnitId,
                    CAST(NULL AS NVARCHAR(200)) AS FromUnitName,
                    A.ToUnitId,
                    CAST(NULL AS NVARCHAR(200)) AS ToUnitName,
                    A.FromDepartmentId,
                    CAST(NULL AS NVARCHAR(200)) AS FromDepartment,
                    A.ToDepartmentId,
                    CAST(NULL AS NVARCHAR(200)) AS ToDepartment,
                    A.FromCustodianName,
                    A.ToCustodianName,
                    RH.Sdcno,
                    A.GatePassNo,
                    RH.Remarks
                FROM FixedAsset.AssetTransferIssueHdr A
                INNER JOIN FixedAsset.AssetTransferIssueDtl B ON A.Id = B.AssetTransferId
                INNER JOIN FixedAsset.AssetMaster M ON B.AssetId = M.Id
                INNER JOIN FixedAsset.MiscMaster C ON A.TransferType = C.Id
                LEFT JOIN FixedAsset.AssetTransferReceiptHdr RH ON A.Id = RH.AssetTransferId
                LEFT JOIN FixedAsset.AssetTransferReceiptDtl RD ON RH.Id = RD.AssetReceiptId AND B.AssetId = RD.AssetId
                WHERE A.Status = 'Approved' AND A.ToUnitId = @UnitId
                AND (RD.AckStatus = 0 OR RD.AckStatus IS NULL)
                AND A.Id = @assetTransferId
                FOR JSON PATH, INCLUDE_NULL_VALUES;

                SELECT
                B.AssetId,
                M.AssetCode,
                M.AssetName
                FROM FixedAsset.AssetTransferIssueHdr A
                INNER JOIN FixedAsset.AssetTransferIssueDtl B ON A.Id = B.AssetTransferId
                INNER JOIN FixedAsset.AssetMaster M ON B.AssetId = M.Id
                INNER JOIN FixedAsset.MiscMaster C ON A.TransferType = C.Id
                LEFT JOIN FixedAsset.AssetTransferReceiptHdr RH ON A.Id = RH.AssetTransferId
                LEFT JOIN FixedAsset.AssetTransferReceiptDtl RD ON RH.Id = RD.AssetReceiptId AND B.AssetId = RD.AssetId
                WHERE A.Status = 'Approved'  AND A.ToUnitId = @UnitId
                AND (RD.AckStatus = 0 OR RD.AckStatus IS NULL)
        		AND A.Id=@assetTransferId
                FOR JSON PATH, INCLUDE_NULL_VALUES;
                ";

        using var multiQuery = await _dbConnection.QueryMultipleAsync(query, new { assetTransferId, UnitId });

        string headerJson = await multiQuery.ReadFirstOrDefaultAsync<string>();
        string detailsJson = await multiQuery.ReadFirstOrDefaultAsync<string>();

        if (string.IsNullOrWhiteSpace(headerJson))
        {
            return null;
        }

        var header = JsonSerializer.Deserialize<List<AssetTrasnferReceiptHdrPendingDto>>(headerJson, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        })?.FirstOrDefault();

        var details = JsonSerializer.Deserialize<List<AssetTransferReceiptDtlPendingDto>>(detailsJson ?? "[]", new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (header != null)
        {
            header.AssetTransferPendingDtl = details ?? new List<AssetTransferReceiptDtlPendingDto>();
        }

        return header;
        }


        // public async Task<List<AssetTransferReceiptDtlPendingDto>> GetAllPendingAssetTransferDtlAsync(int assetTransferId)
        // {
        //      const string query = @"
                // SELECT 
                // A.Id AS AssetTransferId,
                // B.AssetId,
                // M.AssetCode,
                // M.AssetName
                // FROM FixedAsset.AssetTransferIssueHdr A
                // INNER JOIN FixedAsset.AssetTransferIssueDtl B ON A.Id = B.AssetTransferId
                // INNER JOIN FixedAsset.AssetMaster M ON B.AssetId = M.Id
                // INNER JOIN FixedAsset.MiscMaster C ON A.TransferType = C.Id
                // INNER JOIN [BannariERP].AppData.Unit D ON A.FromUnitId = D.Id
                // INNER JOIN [BannariERP].AppData.Unit E ON A.ToUnitId = E.Id
                // INNER JOIN [BannariERP].AppData.Department F ON A.FromDepartmentId = F.Id
                // INNER JOIN [BannariERP].AppData.Department G ON A.ToDepartmentId = G.Id
                // LEFT JOIN FixedAsset.AssetTransferReceiptHdr RH ON A.Id = RH.AssetTransferId
                // LEFT JOIN FixedAsset.AssetTransferReceiptDtl RD ON RH.Id = RD.AssetReceiptId AND B.AssetId = RD.AssetId
                // WHERE A.Status = 'Approved' 
                // AND (RD.AckStatus = 0 OR RD.AckStatus IS NULL)
        		// AND A.Id=@assetTransferId";

        //     var assetreceiptpendingdtl = await _dbConnection.QueryAsync<AssetTransferReceiptDtlPendingDto>(query, new { assetTransferId });

        //     return assetreceiptpendingdtl.ToList();
        // }
    }
}