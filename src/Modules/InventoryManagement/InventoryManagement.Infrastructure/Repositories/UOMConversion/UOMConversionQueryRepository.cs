using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using InventoryManagement.Application.Common.Interfaces.IUOMConversion;
using InventoryManagement.Application.UOMConversion.Queries.GetAllUOMConversion;
using Dapper;

namespace InventoryManagement.Infrastructure.Repositories.UOMConversion
{
    public class UOMConversionQueryRepository : IUOMConversionQueryRepository
    {
        private readonly IDbConnection _dbConnection;

      

        public UOMConversionQueryRepository(IDbConnection dbConnection)
        {

            _dbConnection = dbConnection;
            
        }
        public async Task<(List<UOMConversionDto>, int)> GetAllUOMConversionAsync(int PageNumber, int PageSize, string? SearchTerm)
        {
            var query = $$"""
                DECLARE @TotalCount INT;

               
                SELECT @TotalCount = COUNT(*)
                FROM Inventory.UOMConversion UC
                INNER JOIN Inventory.UOM FU ON FU.Id = UC.FromUOMId
                INNER JOIN Inventory.UOM TU ON TU.Id = UC.ToUOMId
                WHERE UC.IsDeleted = 0
                {{(string.IsNullOrEmpty(SearchTerm) ? "" : "AND (FU.Code LIKE @Search OR TU.Code LIKE @Search)")}};

               
                SELECT 
                    UC.Id,
                    UC.FromUOMId,
                    FU.Code AS FromUOMCode,
                    UC.ToUOMId,
                    TU.Code AS ToUOMCode,
                    UC.ConversionValue,
                    UC.IsActive,
                    UC.IsDeleted,
                    UC.CreatedBy,
                    UC.CreatedDate,
                    UC.CreatedByName,
                    UC.CreatedIP,
                    UC.ModifiedBy,
                    UC.ModifiedDate,
                    UC.ModifiedByName,
                    UC.ModifiedIP
                FROM Inventory.UOMConversion UC
                INNER JOIN Inventory.UOM FU ON FU.Id = UC.FromUOMId
                INNER JOIN Inventory.UOM TU ON TU.Id = UC.ToUOMId
                WHERE UC.IsDeleted = 0
                {{(string.IsNullOrEmpty(SearchTerm) ? "" : "AND (FU.Code LIKE @Search OR TU.Code LIKE @Search)")}}
                ORDER BY UC.Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                
                SELECT @TotalCount AS TotalCount;
            """;

            var parameters = new
            {
                Search = string.IsNullOrEmpty(SearchTerm) ? null : $"%{SearchTerm}%",
                Offset = (PageNumber - 1) * PageSize,
                PageSize
            };

            // var result = await _dbConnection.QueryMultipleAsync(query, parameters);

            var result = await _dbConnection.QueryMultipleAsync(query, parameters);
            var multi = await _dbConnection.QueryMultipleAsync(query, parameters);
            var conversionList = (await multi.ReadAsync<UOMConversionDto>()).ToList();
            var totalCount = await multi.ReadFirstAsync<int>();

            return (conversionList, totalCount);

            // var conversionList = (await result.ReadAsync<Core.Domain.Entities.UOMConversion>()).ToList();
            // int totalCount = await result.ReadFirstAsync<int>();

            // return (conversionList, totalCount);
        }

        public async Task<UOMConversionDto> GetByIdAsync(int id)
        {
            const string query = @"
                SELECT 
                    UC.Id,
                    UC.FromUOMId,
                    FU.Code AS FromUOMCode,
                    UC.ToUOMId,
                    TU.Code AS ToUOMCode,
                    UC.ConversionValue,
                    UC.IsActive,
                    UC.IsDeleted,
                    UC.CreatedBy,
                    UC.CreatedDate,
                    UC.CreatedByName,
                    UC.CreatedIP,
                    UC.ModifiedBy,
                    UC.ModifiedDate,
                    UC.ModifiedByName,
                    UC.ModifiedIP
               FROM Inventory.UOMConversion UC
                INNER JOIN Inventory.UOM FU ON FU.Id = UC.FromUOMId
                INNER JOIN Inventory.UOM TU ON TU.Id = UC.ToUOMId
                WHERE UC.Id = @id AND UC.IsDeleted = 0";

            return await _dbConnection.QueryFirstOrDefaultAsync<UOMConversionDto>(query, new { id });
        }
        
        public async Task<bool> AlreadyExistsAsync(int fromUOMId, int toUOMId, int? id = null)
        {
            var query = @"
                SELECT COUNT(1)
                FROM Inventory.UOMConversion
                WHERE FromUOMId = @FromUOMId
                AND ToUOMId = @ToUOMId
                AND IsDeleted = 0";

            var parameters = new DynamicParameters(new
            {
                FromUOMId = fromUOMId,
                ToUOMId = toUOMId
            });

            if (id.HasValue)
            {
                query += " AND Id != @Id";
                parameters.Add("Id", id);
            }

            var count = await _dbConnection.ExecuteScalarAsync<int>(query, parameters);
            return count > 0;
        }
        public async Task<decimal?> GetConversionFactorAsync(int fromUOMId, int toUOMId)
        {
            const string query = @"
                SELECT ConversionValue
                FROM Inventory.UOMConversion
                WHERE FromUOMId = @FromUOMId 
                AND ToUOMId = @ToUOMId
                AND IsDeleted = 0
                AND IsActive = 1";

            var result = await _dbConnection.QueryFirstOrDefaultAsync<decimal?>(query, new { FromUOMId = fromUOMId, ToUOMId = toUOMId });
            return result;
        }


    }
}