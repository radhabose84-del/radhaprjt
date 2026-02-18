#nullable disable
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Users;
using WarehouseManagement.Application.Common.Interfaces.IWarehouseMaster;
using Microsoft.EntityFrameworkCore;
using WarehouseManagement.Infrastructure.Data;

namespace WarehouseManagement.Application.WarehouseMaster.Services
{
    public class WarehouseCodeGenerator : IWarehouseCodeGenerator
    {
        private readonly IUnitLookup _unitLookup;
        private readonly IMiscMasterLookup _miscMasterLookup;
        private readonly ApplicationDbContext _dbContext;
        public WarehouseCodeGenerator(IUnitLookup unitLookup, IMiscMasterLookup miscMasterLookup, ApplicationDbContext dbContext)
        {
            _unitLookup = unitLookup;
            _miscMasterLookup = miscMasterLookup;
            _dbContext = dbContext;
        }
          public async Task<string> GenerateAsync(int unitId, int warehouseTypeId)
        {
            // Use only for CREATE
            var prefix = await BuildPrefixAsync(unitId, warehouseTypeId);
            var next   = await GetNextNumberAsync(unitId, warehouseTypeId);
            return $"{prefix}-{next:D4}";
        }

        /// <summary>
        /// Rebuilds code for UPDATE. Preserves old suffix when possible, otherwise allocates next.
        /// </summary>
        public async Task<string> RebuildForUpdateAsync(int unitId, int warehouseTypeId, string existingCode, int existingId)
        {
            var prefix = await BuildPrefixAsync(unitId, warehouseTypeId);
            var keepSuffix = TryExtractSuffix(existingCode) ?? 0;

            string candidate = keepSuffix > 0 ? $"{prefix}-{keepSuffix:D4}" : null;

            // If we have a candidate using the old suffix, ensure it doesn't collide with another row
            if (!string.IsNullOrEmpty(candidate))
            {
                var taken = await _dbContext.WarehouseMasters
                    .AnyAsync(w => w.WarehouseCode == candidate && w.Id != existingId);

                if (!taken)
                    return candidate;
            }

            // Fallback: allocate next number in the new partition
            var next = await GetNextNumberAsync(unitId, warehouseTypeId);
            return $"{prefix}-{next:D4}";
        }

        private async Task<string> BuildPrefixAsync(int unitId, int warehouseTypeId)
        {
            var units = await _unitLookup.GetAllUnitAsync();
            var unitShortName = units.FirstOrDefault(u => u.UnitId == unitId)?.ShortName ?? "00";

            var miscMasters = await _miscMasterLookup.GetMiscMasterByIdAsync("WarehouseType");
            var miscCode = miscMasters.FirstOrDefault(x => x.Id == warehouseTypeId)?.Code ?? "XX";

            return $"WH-{unitShortName}-{miscCode}";
        }
        private async Task<int> GetNextNumberAsync(int unitId, int warehouseTypeId)
        {
            // Open the same connection EF uses
            var conn = _dbContext.Database.GetDbConnection();
            if (conn.State != ConnectionState.Open)
                await conn.OpenAsync();

            await using var tx = await conn.BeginTransactionAsync(IsolationLevel.Serializable);
            try
            {
                await using var cmd = conn.CreateCommand();
                cmd.Transaction = tx;

                // Lock the qualifying rows/range so concurrent callers serialize
                cmd.CommandText = @"
                    SELECT ISNULL(MAX(CAST(RIGHT(WarehouseCode, 4) AS int)), 0) + 1
                    FROM Warehouse.WarehouseMaster WITH (UPDLOCK, HOLDLOCK, ROWLOCK)
                    WHERE UnitId = @unitId
                    AND WarehouseTypeId = @warehouseTypeId
                    AND IsDeleted = 0
                    AND WarehouseCode IS NOT NULL
                    AND LEN(WarehouseCode) >= 4
                    AND RIGHT(WarehouseCode, 4) NOT LIKE '%[^0-9]%';
                ";

                var p1 = cmd.CreateParameter(); p1.ParameterName = "@unitId"; p1.Value = unitId; cmd.Parameters.Add(p1);
                var p2 = cmd.CreateParameter(); p2.ParameterName = "@warehouseTypeId"; p2.Value = warehouseTypeId; cmd.Parameters.Add(p2);

                var result = await cmd.ExecuteScalarAsync();
                var next = Convert.ToInt32(result);

                await tx.CommitAsync();
                return next;
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }      

        private static int? TryExtractSuffix(string code)
        {
            if (string.IsNullOrWhiteSpace(code) || code.Length < 4) return null;
            var tail = code.Substring(code.Length - 4);
            return int.TryParse(tail, out var n) ? n : (int?)null;
        }
    

    }
}
