using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceManagement.Application.Common.Interfaces.IMRS;
using MaintenanceManagement.Application.MRS.Queries;
using MaintenanceManagement.Application.MRS.Queries.GetCategory;
using MaintenanceManagement.Application.MRS.Queries.GetDepartment;
using MaintenanceManagement.Application.MRS.Queries.GetPendingQty;
using MaintenanceManagement.Application.MRS.Queries.GetSubCostCenter;
using MaintenanceManagement.Application.MRS.Queries.GetSubDepartment;
using Dapper;

namespace MaintenanceManagement.Infrastructure.Repositories.MRS
{
    public class MRSQueryRepository : IMRSQueryRepository
    {
        private readonly IDbConnection _dbConnection;
        public MRSQueryRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<List<MCategoryDto>> GetCategory(string OldUnitcode)
        {
               OldUnitcode ??= string.Empty;

            var parameters = new 
            {
                OldUnitcode
            };

            var departments = await _dbConnection.QueryAsync<MCategoryDto>(
                "GetCategoryByOldUnitcode", // Stored Procedure Name
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return departments.ToList();
        }

        public async Task<List<MDepartmentDto>> GetMDepartment(string OldUnitcode)
        {
            OldUnitcode ??= string.Empty;

            var parameters = new 
            {
                OldUnitcode
            };

            var departments = await _dbConnection.QueryAsync<MDepartmentDto>(
                "GetDepartmentsByOldUnitcode", // Stored Procedure Name
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return departments.ToList();
        }

        public async Task<GetPendingQtyDto?> GetPendingIssueAsync(string oldUnitCode, string itemCode)
        {
            oldUnitCode ??= string.Empty;
            itemCode ??= string.Empty;

            var parameters = new 
            {
                OldUnitcode = oldUnitCode,
                ItemCode = itemCode
            };

            var result = await _dbConnection.QueryFirstOrDefaultAsync<GetPendingQtyDto>(
                "KalsoftePrimeIssueRequestPending", // Stored Procedure Name
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return result;
        }

        public async Task<List<MSubCostCenterDto>> GetSubCostCenter(string OldUnitcode)
        {
            OldUnitcode ??= string.Empty;

            var parameters = new 
            {
                OldUnitcode
            };

            var departments = await _dbConnection.QueryAsync<MSubCostCenterDto>(
                "GetSubCostCentersByOldUnitcode", // Stored Procedure Name
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return departments.ToList();
        }

        public async Task<List<MSubDepartment>> GetSubDepartment(string OldUnitcode)
        {
             OldUnitcode ??= string.Empty;

            var parameters = new 
            {
                OldUnitcode
            };

            var departments = await _dbConnection.QueryAsync<MSubDepartment>(
                "GetSubDepartmentByOldUnitcode", // Stored Procedure Name
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return departments.ToList();
        }
    }
}