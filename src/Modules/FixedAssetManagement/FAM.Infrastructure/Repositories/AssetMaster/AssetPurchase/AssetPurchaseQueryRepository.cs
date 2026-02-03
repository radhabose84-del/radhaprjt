using System;
using System.Collections.Generic;
using System.Data;
using System.Formats.Asn1;
using System.Linq;
using System.Threading.Tasks;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetPurchase;
using FAM.Domain.Entities;
using FAM.Domain.Entities.AssetPurchase;
using Dapper;
using Serilog;

namespace FAM.Infrastructure.Repositories.AssetMaster.AssetPurchase
{
    public class AssetPurchaseQueryRepository : IAssetPurchaseQueryRepository
    {
         private readonly IDbConnection _dbConnection; 

        public AssetPurchaseQueryRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<(List<AssetPurchaseDetails>, int)> GetAllPurchaseDetails(int PageNumber, int PageSize, string? SearchTerm)
        {
            var query = $$"""
             DECLARE @TotalCount INT;
             SELECT @TotalCount = COUNT(*) 
               FROM FixedAsset.AssetPurchaseDetails
            {{(string.IsNullOrEmpty(SearchTerm) ? "" : "AND (ItemName LIKE @Search OR GrnNo LIKE @Search)")}};

                SELECT 
                Id,
                AssetId,
                AssetSourceId,
                BudgetType,
                OldUnitId,
                VendorCode,
                VendorName,
                PoDate,
                PoNo,
                PoSno,
                ItemCode,
                ItemName,
                GrnNo,
                GrnSno,
                GrnDate,
                QcCompleted,
                AcceptedQty,
                PurchaseValue,
                GrnValue,
                BillNo,
                BillDate,
                Uom,
                BinLocation,
                PjYear,
                PjDocId,
                PjDocSr,
                PjDocNo,
                CapitalizationDate
            FROM FixedAsset.AssetPurchaseDetails 
                {{(string.IsNullOrEmpty(SearchTerm) ? "" : "AND (ItemName LIKE @Search OR GrnNo LIKE @Search )")}}
                ORDER BY Id desc
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                SELECT @TotalCount AS TotalCount;
            """;

            
             var parameters = new
                       {
                           Search = $"%{SearchTerm}%",
                           Offset = (PageNumber - 1) * PageSize,
                           PageSize
                       };

             var assetpurchase = await _dbConnection.QueryMultipleAsync(query, parameters);
             var assetpurchaselist = (await assetpurchase.ReadAsync<FAM.Domain.Entities.AssetPurchase.AssetPurchaseDetails>()).ToList();
             int totalCount = (await assetpurchase.ReadFirstAsync<int>());
             return (assetpurchaselist, totalCount);
        }

       public async Task<List<AssetGrnItem>> GetAssetGrnItem(string OldUnitId,int AssetSourceId ,int GrnNo)
        {
            //Prime
        if(AssetSourceId == 2)
        {
            const string query = @"
            SELECT grnslno as GrnSerialNo,idesc as ItemName
            FROM dbo.GetGRNSNO(@OldUnitId,@GrnNo,NULL) order by grnslno asc";

            var parameters = new { OldUnitId, GrnNo };
            var grnList = await _dbConnection.QueryAsync<AssetGrnItem>(query, parameters);

            return grnList?.ToList() ?? new List<AssetGrnItem>();
        }

        //Kalsofte
        else if (AssetSourceId == 1)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@DivCode", OldUnitId.ToString(), DbType.String);
            parameters.Add("@DocNo", GrnNo, DbType.Int32);
            var grnList = await _dbConnection.QueryAsync<AssetGrnItem>(
            "dbo.GetGRNByItemDivision", 
            parameters, 
            commandType: CommandType.StoredProcedure,commandTimeout: 120
        );

        if (!grnList.Any())
        {
        Log.Information("No data returned from stored procedure!");
        }

        return grnList?.ToList() ?? new List<AssetGrnItem>();

        }
        else
        {
             return new List<AssetGrnItem>();
        }
        }

        public async Task<List<AssetGrnDetails>> GetAssetGrnItemDetails(string OldUnitId,int AssetSourceId ,int GrnNo, int GrnSerialNo)
        {
             //Prime
        if(AssetSourceId == 2)
        {
             const string query = @"
            SELECT
            budtype as BudgetType, 
            unitcode as OldUnitId,
            slcode as VendorCode,
            slname as VendorName,
            podt as PoDate,
            pono as PoNo,
            poslno as PoSno,
            itemcode as ItemCode,
            idesc as ItemName,
            grnno as GrnNo,
            grnslno as GrnSno,
            grndt as GrnDate,
            qcflg as QcCompleted,
            acpqty as AcceptedQty,
            itmval as PurchaseValue,
            grnval as GrnValue,
            billno as BillNo,
            billdt as BillDate,
            iuom as Uom,
            binloc as BinLocation,
            acyr as PjYear,
            docid as PjDocId,
            docsr as PjDocSr,
            docno as PjDocNo
            FROM 
            dbo.GetGRNSNO(@OldUnitId,@GrnNo,@GrnSerialNo)";

            var parameters = new { OldUnitId, GrnNo, GrnSerialNo };
            var grnList = await _dbConnection.QueryAsync<AssetGrnDetails>(query, parameters);

            return grnList?.ToList() ?? new List<AssetGrnDetails>();
        }

        //Kalsofte
        else if (AssetSourceId == 1)
        { 
            var parameters = new DynamicParameters();
            parameters.Add("@OldUnitId ", OldUnitId.ToString(), DbType.String);
            parameters.Add("@GrnNo", GrnNo, DbType.Int32);
            parameters.Add("@GrnSno", GrnSerialNo, DbType.Int32);
            var grnList = await _dbConnection.QueryAsync<AssetGrnDetails>(
            "dbo.GetPurchaseDetailsKalsofte", 
            parameters, 
            commandType: CommandType.StoredProcedure,commandTimeout: 120
        );
        if (!grnList.Any())
        {
        Log.Information("No data returned from stored procedure!");
        }

        return grnList?.ToList() ?? new List<AssetGrnDetails>();
        }
        else
        {
            return new List<AssetGrnDetails>();
        }
        }

    
        public async Task<List<AssetGrn>> GetAssetGrnNo(string OldUnitId, int AssetSourceId,string? SearchGrnNo)
        {

        //Prime
        if(AssetSourceId == 2)
        {
            var query = @"
        SELECT DISTINCT grnno AS GrnNo, unitcode AS OldUnitId
        FROM dbo.GetGRNDetails(@OldUnitId)";

        if (!string.IsNullOrWhiteSpace(SearchGrnNo))
        {
        query += " WHERE grnno LIKE @SearchGrnNo";
        }

        var parameters = new 
        { 
        OldUnitId, 
        SearchGrnNo = $"%{SearchGrnNo}%" // Enables partial search
        };
        var grnList = await _dbConnection.QueryAsync<AssetGrn>(query, parameters);
        return grnList?.ToList() ?? new List<AssetGrn>();
        }

        //Kalsofte
        else if (AssetSourceId == 1)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@DivCode", OldUnitId.ToString(), DbType.String);
            parameters.Add("@DocNo", string.IsNullOrWhiteSpace(SearchGrnNo) ? (object)DBNull.Value : $"%{SearchGrnNo}%", DbType.String);
            var grnList = await _dbConnection.QueryAsync<AssetGrn>(
            "dbo.GetGRNByDivision", 
            parameters, 
            commandType: CommandType.StoredProcedure,commandTimeout: 120
        );

        if (!grnList.Any())
        {
        Log.Information("No data returned from stored procedure!");
        }

        return grnList?.ToList() ?? new List<AssetGrn>();
        }
        else
        {
            return new List<AssetGrn>();
        }
        }   

        public async Task<List<AssetSource>> GetAssetSources(string searchPattern)
        {
            searchPattern = searchPattern ?? string.Empty; // Prevent null issues

            const string query = @"
             SELECT Id, SourceName 
            FROM FixedAsset.AssetSource 
            WHERE IsDeleted = 0 
            AND SourceName LIKE @SearchPattern";  
            var parameters = new 
            { 
            SearchPattern = $"%{searchPattern}%" 
            };

            var assetSources = await _dbConnection.QueryAsync<FAM.Domain.Entities.AssetSource>(query, parameters);
            return assetSources.ToList();
        }

        public async Task<List<AssetUnit>> GetAssetUnit(string userName)
        {
         

            const string query = @"
            SELECT C.OldUnitId, C.UnitName 
            FROM BannariERP.AppSecurity.Users A
            INNER JOIN BannariERP.AppSecurity.UserUnit B ON A.UserId = B.UserId
            INNER JOIN BannariERP.AppData.Unit C ON B.UnitId = C.Id
            WHERE B.IsActive = 1 
            AND A.IsActive = 1 
            AND A.UserName = @UserName
             ORDER BY C.UnitName ASC";
    
             var parameters = new { UserName = userName };

            var assetSources = await _dbConnection.QueryAsync<FAM.Domain.Entities.AssetPurchase.AssetUnit>(query, parameters);
            return assetSources.ToList();
        }

        public async Task<AssetPurchaseDetails?> GetByIdAsync(int Id)
        {
             const string query = @"
                    SELECT * 
                    FROM FixedAsset.AssetPurchaseDetails 
                    WHERE AssetId = @Id";

                    var assetPurchaseDetails = await _dbConnection.QueryFirstOrDefaultAsync<FAM.Domain.Entities.AssetPurchase.AssetPurchaseDetails>(query, new { Id });
                    return assetPurchaseDetails;
        }
    }
}