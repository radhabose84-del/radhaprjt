using Microsoft.EntityFrameworkCore;
using UserManagement.Infrastructure.Data;
using Core.Domain.Entities;
using Core.Application.Common.Interfaces.IUnit;
using System.Data;
using Dapper;
using Core.Application.Units.Queries.GetUnits;
using Core.Application.Common.Interfaces;

namespace UserManagement.Infrastructure.Repositories.Units
{
  public class UnitQueryRepository : IUnitQueryRepository, IUnitGrpcQuery
  {
    private readonly IDbConnection _dbConnection;

    private readonly IIPAddressService _ipAddressService;

    public UnitQueryRepository(IDbConnection dbConnection, IIPAddressService ipAddressService)
    {
      _dbConnection = dbConnection;
      _ipAddressService = ipAddressService;
    }

    public async Task<(List<Unit>, int)> GetAllUnitsAsync(int PageNumber, int PageSize, string? SearchTerm)
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
            C.SpindlesCapacity
             FROM AppData.Unit C
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
                U.DivisionId
            FROM AppData.Unit U
            Inner join [AppSecurity].[UserUnit] UU on UU.UnitId = U.Id where IsDeleted = 0 
            and UnitName like @SearchPattern and UU.UserId = @UserId and UU.IsActive = 1 and U.CompanyId = @CompanyId";

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
                U.DivisionId
            FROM AppData.Unit U
            Inner join [AppSecurity].[UserUnit] UU on UU.UnitId = U.Id 
            where U.IsDeleted = 0 
            and UU.UserId = @UserId and UU.IsActive = 1 and U.CompanyId = @CompanyId";

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
      var companyId = _ipAddressService.GetCompanyId();
      const string query = @"
                     SELECT 
                     U.Id, 
                     U.UnitName,
                     U.DivisionId
                 FROM AppData.Unit U
                 WHERE IsDeleted = 0 AND UnitName like @SearchPattern AND U.CompanyId=@CompanyId";

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
            C.SpindlesCapacity
             FROM AppData.Unit C
             Inner join [AppSecurity].[UserUnit] UU on UU.UnitId = C.Id where IsDeleted = 0 
              WHERE C.IsDeleted = 0 AND UU.UserId = @UserId and UU.IsActive = 1
               
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
                 B.PhoneNo ,
                 B.Remarks As Remarks     
             FROM AppData.Unit C
             LEFT JOIN AppData.UnitAddress A ON A.UnitId = C.Id
             LEFT JOIN AppData.UnitContacts B ON B.UnitId = C.Id
			       LEFT JOIN AppData.Company D ON D.Id = C.CompanyId
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

  }
    
    }
