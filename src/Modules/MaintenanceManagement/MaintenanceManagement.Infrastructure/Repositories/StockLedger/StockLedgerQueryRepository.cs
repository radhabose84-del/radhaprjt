using System.Data;
using MaintenanceManagement.Application.Common.Interfaces.IStcokLedger;
using MaintenanceManagement.Application.StockLedger.Queries.GetCurrentStock;
using MaintenanceManagement.Application.StockLedger.Queries.GetCurrentStockItemsById;
// using MaintenanceManagement.Application.StockLedger.Queries.GetStockLegerReport;
using Dapper;

namespace MaintenanceManagement.Infrastructure.Repositories.StockLedger
{
    public class StockLedgerQueryRepository : IStockLedgerQueryRepository
    {
        private readonly IDbConnection _dbConnection;
        public StockLedgerQueryRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }


        public async Task<List<StockItemCodeDto>> GetStockItemCodes(string OldUnitcode,int DepartmentId)
        {
            OldUnitcode = OldUnitcode ?? string.Empty; // Prevent null issues

            const string query = @"
                SELECT 
                    ItemCode,
                    ItemName
                FROM 
                    Maintenance.StockLedger
                WHERE
                    Oldunitcode = @OldUnitcode
                    AND TransactionType not in('SRP','REU') 
                    AND DepartmentId = @DepartmentId
                GROUP BY 
                    ItemCode, ItemName, Oldunitcode,DepartmentId
                HAVING
                    SUM(ReceivedQty) - SUM(IssueQty) > 0";

            var parameters = new
            {
                OldUnitcode,
                DepartmentId // match exactly, no wildcards
            };

            var itemcodes = await _dbConnection.QueryAsync<StockItemCodeDto>(query, parameters);
            return itemcodes.ToList();
        }
        public async Task<List<StockItemCodeDto>> GetAllItemCodes(string OldUnitcode,int DepartmentId)
        {
            OldUnitcode = OldUnitcode ?? string.Empty; // Prevent null issues

            const string query = @"
                SELECT 
                    ItemCode,
                    ItemName
                FROM 
                    Maintenance.StockLedger
                WHERE
                    Oldunitcode = @OldUnitcode
                    AND TransactionType not in('SRP','REU') 
                    AND DepartmentId = @DepartmentId      ";

            var parameters = new
            {
                OldUnitcode,
                DepartmentId // match exactly, no wildcards
            };

            var itemcodes = await _dbConnection.QueryAsync<StockItemCodeDto>(query, parameters);
            return itemcodes.ToList();
        }
        public async Task<CurrentStockDto?> GetSubStoresCurrentStock(string OldUnitcode, string Itemcode,int DepartmentId)
        {
            const string query = @"
                SELECT 
                    Oldunitcode as OldUnitId,
                    ItemCode,
                    ItemName,
                    SUM(ReceivedQty) - SUM(IssueQty) AS StockQty,
                    Uom,
                    SUM(ReceivedValue) - SUM(IssueValue) AS StockValue,
                    ((SUM(ReceivedValue) - SUM(IssueValue)) / (SUM(ReceivedQty) - SUM(IssueQty))) AS Rate
                FROM 
                    Maintenance.StockLedger
                WHERE
                    Oldunitcode = @OldUnitcode AND
                    ItemCode = @Itemcode AND
                    DepartmentId = @DepartmentId AND
                    TransactionType not in('SRP','REU')
                GROUP BY 
                    ItemCode, ItemName, Oldunitcode,Uom,DepartmentId
                HAVING
                    SUM(ReceivedQty) - SUM(IssueQty) > 0";

            var parameters = new { OldUnitcode = OldUnitcode, Itemcode = Itemcode, DepartmentId = DepartmentId };

            var stocklist = await _dbConnection.QueryFirstOrDefaultAsync<CurrentStockDto>(query, parameters);

            return stocklist;
        }

       
    }
}