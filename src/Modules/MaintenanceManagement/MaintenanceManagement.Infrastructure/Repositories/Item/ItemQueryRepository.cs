using System.Data;
using MaintenanceManagement.Application.Common.Interfaces.IItem;
using MaintenanceManagement.Domain.Entities;
using Dapper;
using Serilog;

namespace MaintenanceManagement.Infrastructure.Repositories.Item
{
    public class ItemQueryRepository : IItemQueryRepository
    {
        private readonly IDbConnection _dbConnection; 

        public ItemQueryRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }
        public async Task<List<ItemGroupCode>> GetGroupCodes(string OldUnitId)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@OldunitCode", OldUnitId.ToString(), DbType.String);
            var itemGroupCodes = await _dbConnection.QueryAsync<ItemGroupCode>(
            "[dbo].[GetGroupCode]", 
            parameters, 
            commandType: CommandType.StoredProcedure
            );
            if (!itemGroupCodes.Any())
            {
            Log.Information("No data returned from stored procedure!");
            }
            return itemGroupCodes?.ToList() ?? new List<ItemGroupCode>();
        }

        public async Task<List<ItemMaster>> GetItemMasters(string OldUnitId, string Grpcode, string? ItemCode, string? ItemName)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@OldunitCode", OldUnitId);
            parameters.Add("@Grpcode", Grpcode);
            parameters.Add("@ItemCode", string.IsNullOrWhiteSpace(ItemCode) ? null : ItemCode);
            parameters.Add("@ItemName", string.IsNullOrWhiteSpace(ItemName) ? null : ItemName);

            var result = await _dbConnection.QueryAsync<ItemMaster>(
                "dbo.GetItemsByGroupCode",
                parameters,
                commandType: CommandType.StoredProcedure);

            if (!result.Any())
            {
            Log.Information("No data returned from stored procedure!");
            }

            return result.ToList()?? new List<ItemMaster>();
        }
    }
}