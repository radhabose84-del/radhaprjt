using System.Data;
using Contracts.Interfaces;
using PurchaseManagement.Application.Common.Interfaces;
using PurchaseManagement.Application.Common.Interfaces.IIssueReturn;
using PurchaseManagement.Application.IssueReturn.Queries.GetIssueDetailsById;
using PurchaseManagement.Application.IssueReturn.Queries.GetIssueReturnDetailsById;
using PurchaseManagement.Domain.Common;
using Dapper;

namespace PurchaseManagement.Infrastructure.Repositories.IssueReturn
{
    public class IssueReturnEntryQueryRepository : IIssueReturnEntryQueryRepository
    {
        private readonly IDbConnection _dbConnection;
        private readonly IIPAddressService _ipAddressService;
        public IssueReturnEntryQueryRepository(IDbConnection dbConnection, IIPAddressService ipAddressService)
        {
            _dbConnection = dbConnection;
            _ipAddressService = ipAddressService;
        }

       public async Task<GetIssueReturnDetailsByIdDto> GetByIdWithDetails(int id)
        {
            var sql = @"
                SELECT A.*,B.Description AS RequestCategoryName, C.Description AS StatusName
                FROM 
                Purchase.IssueReturnHeader A 
                INNER JOIN Purchase.MiscMaster B ON A.RequestCategoryId = B.Id
                INNER JOIN Purchase.MiscMaster C ON A.StatusId = C.Id
                WHERE A.Id = @Id;

                SELECT D.*, E.Description AS StatusName
                FROM Purchase.IssueReturnDetail D
                INNER JOIN Purchase.MiscMaster E ON D.StatusId = E.Id
                WHERE D.IssueReturnHeaderId = @Id;
            ";

            using (var multi = await _dbConnection.QueryMultipleAsync(sql, new { Id = id }))
            {
                // 1. Read Header
                var header = await multi.ReadSingleAsync<GetIssueReturnDetailsByIdDto>();

                // 2. Read Detail List
                var details = (await multi.ReadAsync<GetIssueReturnDetailsByIdDto.GetIssueReturnDetailsDto>()).ToList();

                // 3. Assign detail list to DTO
                header.getIssueReturnDetails = details;

                return header;
            }
        }

        public async Task<List<GetIssueDetailsByIdDto>> GetIssueDetailsByIssueId(int issueId, int? itemid)
        {
            var UnitId = _ipAddressService.GetUnitId() ?? 0;

            var query = @"
                SELECT 
                    A.Id AS IssueHeaderId,
                    A.IssueDate,
                    A.MrsHeaderId,
                    M.RequestCategoryId,
                    RC.Description AS RequestCategoryName,
                    M.DepartmentId,
                    M.SubDepartmentId,
                    A.UnitId,
                    B.Id AS Sno,
                    B.ItemId,
                    B.UomId,
                    B.WarehouseStockId,
                    B.StorageTypeId,
                    B.TargetId,
                    SUM(B.IssueQuantity) AS TotalIssueQuantity,
                    SUM(B.IssueValue) AS TotalIssueValue,
                    ISNULL(SUM(RD.ReturnQuantity), 0) AS AlreadyReturnQuantity,
                    ISNULL(SUM(RD.ReturnValue), 0) AS AlreadyReturnValue,

                    (SUM(B.IssueQuantity) - ISNULL(SUM(RD.ReturnQuantity), 0)) AS BalanceQuantity,
                    (SUM(B.IssueValue) - ISNULL(SUM(RD.ReturnValue), 0)) AS BalanceIssueValue

                FROM Purchase.IssueHeader A
                INNER JOIN Purchase.IssueDetail B 
                    ON A.Id = B.IssueHeaderId
                INNER JOIN Purchase.MrsHeader M 
                    ON M.Id = A.MrsHeaderId
                INNER JOIN Purchase.MiscMaster RC 
                    ON M.RequestCategoryId = RC.Id
                LEFT JOIN Purchase.IssueReturnHeader RH 
                    ON RH.IssueHeaderId = A.Id
                LEFT JOIN Purchase.IssueReturnDetail RD 
                    ON RD.IssueReturnHeaderId = RH.Id
                AND RD.ItemId = B.ItemId 
                AND RD.UomId = B.UomId
                AND RD.WarehouseStockId = B.WarehouseStockId
                AND RD.StorageTypeId=B.StorageTypeId
				AND RD.TargetId=B.TargetId

                WHERE 
                    A.Id = @IssueId
                    AND (@ItemId IS NULL OR B.ItemId = @ItemId)
                    AND A.UnitId = @UnitId
                    AND RC.Description=@RequestCategory

                GROUP BY 
                A.Id,
                A.MrsHeaderId,
                A.UnitId,
                B.ItemId, 
                B.UomId,
                B.WarehouseStockId,
                M.RequestCategoryId,
                M.DepartmentId,
                M.SubDepartmentId,
                B.StorageTypeId,
                B.TargetId,
                B.Id,
                A.IssueDate,
                RC.Description

            ORDER BY 
                A.Id;";

            var lookup = new Dictionary<int, GetIssueDetailsByIdDto>();

            var result = await _dbConnection.QueryAsync<
                GetIssueDetailsByIdDto,
                GetIssueDetailsByIdDto.GetIssueDetailsByIssueIdDto,
                GetIssueDetailsByIdDto>(
                query,
                (header, detail) =>
                {
                    if (!lookup.TryGetValue(header.IssueHeaderId, out var headerEntry))
                    {
                        headerEntry = header;
                        headerEntry.PendingIssueDetailsByIssueId = new List<GetIssueDetailsByIdDto.GetIssueDetailsByIssueIdDto>();
                        lookup.Add(header.IssueHeaderId, headerEntry);
                    }

                    headerEntry.PendingIssueDetailsByIssueId.Add(detail);
                    return headerEntry;
                },
                new
                {
                    IssueId = issueId,
                    ItemId = itemid,
                    UnitId = UnitId,
                    RequestCategory = MiscEnumEntity.Consumption
                },
                splitOn: "Sno"
            );

            return lookup.Values.ToList();
        }

    }
}