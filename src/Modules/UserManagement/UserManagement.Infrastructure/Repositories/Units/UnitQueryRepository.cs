#nullable disable
using UserManagement.Domain.Entities;
using UserManagement.Application.Common.Interfaces.IUnit;
using System.Data;
using Dapper;
using UserManagement.Application.Units.Queries.GetUnits;
using Contracts.Interfaces;
using UserManagement.Application.Common.Interfaces;

namespace UserManagement.Infrastructure.Repositories.Units
{
    public class UnitQueryRepository : IUnitQueryRepository
  {
    private readonly IDbConnection _dbConnection;

    private readonly IIPAddressService _ipAddressService;

    public UnitQueryRepository(IDbConnection dbConnection, IIPAddressService ipAddressService)
    {
      _dbConnection = dbConnection;
      _ipAddressService = ipAddressService;
    }

    public async Task<(List<Unit>, int)> GetAllUnitsAsync(int PageNumber, int PageSize, string SearchTerm)
    {
      var query = $$"""
            DECLARE @TotalCount INT;
             SELECT @TotalCount = COUNT(*) 
               FROM AppData.Unit C
              WHERE C.IsDeleted = 0
            {{(string.IsNullOrEmpty(SearchTerm) ? "" : "AND (C.UnitName LIKE @Search OR C.ShortName LIKE @Search)")}};

                SELECT
            C.Id,
            C.UnitName,
            C.ShortName,
            C.CompanyId,
            C.DivisionId,
            C.UnitHeadName,
            C.CINNO,
            C.IsActive,
            C.OldUnitId, C.IsMaintenanceStopStart,
            C.SpindlesCapacity,
            C.UnitTypeId,
            MM.Description AS UnitTypeName
             FROM AppData.Unit C
             LEFT JOIN AppData.MiscMaster MM ON MM.Id = C.UnitTypeId AND MM.IsDeleted = 0
              WHERE C.IsDeleted = 0
                {{(string.IsNullOrEmpty(SearchTerm) ? "" : "AND (C.UnitName LIKE @Search OR C.ShortName LIKE @Search)")}}
              ORDER BY C.Id DESC
              OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

             SELECT @TotalCount AS TotalCount;
            """;


      var parameters = new
      {
        Search = $"%{SearchTerm}%",
        Offset = (PageNumber - 1) * PageSize,
        PageSize
      };
      var unit = await _dbConnection.QueryMultipleAsync(query, parameters);
      var unitslits = (await unit.ReadAsync<Unit>()).ToList();
      int totalCount = (await unit.ReadFirstAsync<int>());
      return (unitslits, totalCount);
    }

    // public async Task<Unit> GetByIdAsync(int Id)
    // {
    //   const string query = @"
    //       SELECT 
    //         C.Id, 
    //         C.UnitName, 
    //         C.ShortName,
    //         C.CompanyId,
    //         C.DivisionId,
    //         C.UnitHeadName,
    //         C.CINNO,
    //         C.IsActive,
    //         C.OldUnitId, 
    //         C.IsMaintenanceStopStart,  
    // 	      D.GstNumber AS GstNumber,
    //         A.CountryId,
    //         A.StateId,
    //         A.CityId,
    //         A.AddressLine1,
    //         A.AddressLine2,
    //         A.PinCode,
    //         A.ContactNumber,
    //         A.AlternateNumber,
    //         B.Name,
    //              B.Designation,
    //              B.Email,
    //              B.PhoneNo ,
    //              B.Remarks As Remarks     
    //          FROM AppData.Unit C
    //          LEFT JOIN AppData.UnitAddress A ON A.UnitId = C.Id
    //          LEFT JOIN AppData.UnitContacts B ON B.UnitId = C.Id
    // 	       LEFT JOIN AppData.Company D ON D.Id = C.CompanyId
    //          WHERE C.Id = @id AND C.IsDeleted = 0";
    //   var unitResponse = await _dbConnection.QueryAsync<Unit, UnitAddress, UnitContacts, Unit>(query,
    //   (unit, unitaddress, unitcontacts) =>
    //   {
    //     unit.UnitAddress = unitaddress;
    //     unit.UnitContacts = unitcontacts;
    //     return unit;
    //   },
    //   new { Id },
    //   splitOn: "CountryId,Name");

    //   var unit = unitResponse.FirstOrDefault();
    //   if (unit == null)
    //     return null;

    //   return unit;
    // }

    public async Task<List<Unit>> GetUnit(string searchPattern, int userId, int CompanyId)
    {
      const string query = @"
                SELECT
                U.Id,
                U.UnitName,
                U.DivisionId,
                U.UnitTypeId,
                MM.Description AS UnitTypeName
            FROM AppData.Unit U
            INNER JOIN [AppSecurity].[UserUnit] UU ON UU.UnitId = U.Id AND UU.IsActive = 1
            LEFT JOIN AppData.MiscMaster MM ON MM.Id = U.UnitTypeId AND MM.IsDeleted = 0
            WHERE U.IsDeleted = 0
            AND U.UnitName LIKE @SearchPattern AND UU.UserId = @UserId AND U.CompanyId = @CompanyId";

      var result = await _dbConnection.QueryAsync<Unit>(query, new
      {
        SearchPattern = $"%{searchPattern}%",
        UserId = userId,
        CompanyId = CompanyId

      });
      return result.ToList();
    }
    public async Task<List<Unit>> GetUnitByUserId(int userId, int CompanyId)
    {
      const string query = @"
            SELECT
                U.Id,
                U.UnitName,
                U.DivisionId,
                U.UnitTypeId,
                MM.Description AS UnitTypeName,
                UA.PinCode
            FROM AppData.Unit U
            INNER JOIN [AppSecurity].[UserUnit] UU ON UU.UnitId = U.Id AND UU.IsActive = 1
            LEFT JOIN AppData.MiscMaster MM ON MM.Id = U.UnitTypeId AND MM.IsDeleted = 0
            OUTER APPLY (
                SELECT TOP 1 PinCode
                FROM AppData.UnitAddress
                WHERE UnitId = U.Id
                ORDER BY Id
            ) UA
            WHERE U.IsDeleted = 0
            AND UU.UserId = @UserId AND U.CompanyId = @CompanyId";

      var result = await _dbConnection.QueryAsync<Unit>(query, new
      {
        UserId = userId,
        CompanyId = CompanyId

      });
      return result.ToList();
    }
    public async Task<bool> FKColumnExistValidation(int Id)
    {
      var sql = "SELECT COUNT(1) FROM AppData.Unit WHERE Id = @Id AND IsDeleted = 0 AND IsActive = 1";
      var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = Id });
      return count > 0;
    }
    public async Task<List<Unit>> GetUnit_SuperAdmin(string searchPattern)
    {
      var companyId = _ipAddressService.GetCompanyId() ?? 0;
      const string query = @"
                     SELECT
                     U.Id,
                     U.UnitName,
                     U.DivisionId,
                     U.UnitTypeId,
                     MM.Description AS UnitTypeName
                 FROM AppData.Unit U
                 LEFT JOIN AppData.MiscMaster MM ON MM.Id = U.UnitTypeId AND MM.IsDeleted = 0
                 WHERE U.IsDeleted = 0 AND U.UnitName LIKE @SearchPattern AND U.CompanyId = @CompanyId";

      var result = await _dbConnection.QueryAsync<Unit>(query, new
      {
        SearchPattern = $"%{searchPattern}%",
        CompanyId = companyId

      });
      return result.ToList();
    }

    public async Task<List<Unit>> GetUserUnitsAsync(int UserId)
    {
      var query = $$"""

                SELECT
            C.Id,
            C.UnitName,
            C.ShortName,
            C.CompanyId,
            C.DivisionId,
            C.UnitHeadName,
            C.CINNO,
            C.IsActive,
            C.OldUnitId,C.IsMaintenanceStopStart,
            C.SpindlesCapacity,
            C.UnitTypeId,
            MM.Description AS UnitTypeName
             FROM AppData.Unit C
             INNER JOIN [AppSecurity].[UserUnit] UU ON UU.UnitId = C.Id AND UU.IsActive = 1
             LEFT JOIN AppData.MiscMaster MM ON MM.Id = C.UnitTypeId AND MM.IsDeleted = 0
              WHERE C.IsDeleted = 0 AND UU.UserId = @UserId
               
            """;


      var parameters = new
      {
        UserId
      };
      var unit = await _dbConnection.QueryMultipleAsync(query, parameters);
      var unitslits = (await unit.ReadAsync<Unit>()).ToList();
      return unitslits;
    }

    public async Task<GetUnitsByIdDto> GetByIdAsync(int Id)
    {
      const string query = @"
          SELECT
            C.Id,
            C.UnitName,
            C.ShortName,
            C.CompanyId,
            C.DivisionId,
            C.UnitHeadName,
            C.CINNO,
            C.IsActive,
            C.OldUnitId,
            C.IsMaintenanceStopStart,
            C.SpindlesCapacity,
            C.UnitTypeId,
            MM.Description AS UnitTypeName,
            C.BankAccountId,
            D.GstNumber AS GstNumber,
            A.CountryId,
            A.StateId,
            A.CityId,
            A.AddressLine1,
            A.AddressLine2,
            A.PinCode,
            A.ContactNumber,
            A.AlternateNumber,
            B.Name,
                 B.Designation,
                 B.Email,
                 B.PhoneNo,
                 B.Remarks As Remarks
             FROM AppData.Unit C
             LEFT JOIN AppData.UnitAddress A ON A.UnitId = C.Id
             LEFT JOIN AppData.UnitContacts B ON B.UnitId = C.Id
             LEFT JOIN AppData.Company D ON D.Id = C.CompanyId
             LEFT JOIN AppData.MiscMaster MM ON MM.Id = C.UnitTypeId AND MM.IsDeleted = 0
             WHERE C.Id = @id AND C.IsDeleted = 0";
      var unitResponse = await _dbConnection.QueryAsync<GetUnitsByIdDto, UnitAddressDto, UnitContactsDto, GetUnitsByIdDto>(query,
      (getUnitsByIdDto, unitaddressdto, unitcontactsdto) =>
      {
        getUnitsByIdDto.UnitAddressDto = unitaddressdto;
        getUnitsByIdDto.UnitContactsDto = unitcontactsdto;
        return getUnitsByIdDto;
      },
      new { Id },
      splitOn: "CountryId,Name");

      var getUnitsByIdDto = unitResponse.FirstOrDefault();
      if (getUnitsByIdDto == null)
        return null;

      return getUnitsByIdDto;
    }
    public async Task<bool> IsUnitUsedByAnyUserAsync(int unitId)
    {
      const string sql = @"
        SELECT COUNT(1)
        FROM AppSecurity.UserUnit UU
        WHERE UU.UnitId = @UnitId
          AND UU.IsActive = 1";

      var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { UnitId = unitId });
      return count > 0;
    }

    public async Task<bool> MiscMasterExistsAsync(int id)
    {
      const string sql = "SELECT COUNT(1) FROM AppData.MiscMaster WHERE Id = @Id AND IsDeleted = 0 AND IsActive = 1";
      var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = id });
      return count > 0;
    }

    /// <inheritdoc />
    public async Task<bool> SoftDeleteValidationAsync(int id)
    {
      // Block deletion only when a real dependent references this unit (an active user assignment).
      // UnitAddress/UnitContacts are the unit's own 1:1 child records — they are owned by the unit
      // and removed with it, so they must NOT block deletion (otherwise no unit is ever deletable).
      const string sql = @"
        SELECT CASE WHEN
            EXISTS (SELECT 1 FROM [AppSecurity].[UserUnit] WHERE UnitId = @Id AND IsActive = 1)
        THEN 1 ELSE 0 END";

      return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = id });
    }

  }

    }
