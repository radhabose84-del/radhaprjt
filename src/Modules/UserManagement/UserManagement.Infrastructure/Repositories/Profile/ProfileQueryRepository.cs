using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Core.Application.Common.Interfaces.IProfile;
using Core.Domain.Entities;
using Dapper;

namespace UserManagement.Infrastructure.Repositories.Profile
{
    public class ProfileQueryRepository : IProfileQuery
    {
        private readonly IDbConnection _dbConnection;  
        public ProfileQueryRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }
        public async Task<List<Unit>> GetUnit(int userId)
        {
                       const string query = @"
                  SELECT 
                      U.Id, 
                      U.UnitName,
                      U.OldUnitId,
                      U.DivisionId,
                      U.CompanyId,
                      D.Id as DivisionId, 
                      D.Name as Name,
                      D.ShortName as ShortName,
                      C.Id as CompanyId,
                      C.CompanyName
                  FROM AppData.Unit U
                  INNER JOIN [AppSecurity].[UserUnit] UU ON UU.UnitId = U.Id 
                  INNER JOIN [AppData].[Division] D ON D.Id = U.DivisionId
                  INNER JOIN [AppData].[Company] C ON C.Id = U.CompanyId 
                  WHERE U.IsDeleted = 0 AND UU.UserId = @UserId AND UU.IsActive = 1";

              var unitDictionary = new Dictionary<int, Unit>();

              var result = await _dbConnection.QueryAsync<Unit, Division, Company, Unit>(
                  query,
                  (unit, division, company) =>
                  {
                      if (!unitDictionary.TryGetValue(unit.Id, out var existingUnit))
                      {
                          existingUnit = unit;
                          existingUnit.Division = division;
                          existingUnit.Company = company;
                          unitDictionary.Add(existingUnit.Id, existingUnit);
                      }
                      return existingUnit;
                  },
                  new { UserId = userId },
               splitOn: "DivisionId,CompanyId"  
               );

    return result.Distinct().ToList();
        }
    }
}