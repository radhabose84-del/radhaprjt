using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using BSOFT.Infrastructure.Migrations;
using UserManagement.Application.Common.Interfaces.ICustomField;
using UserManagement.Domain.Entities;
using Dapper;

namespace UserManagement.Infrastructure.Repositories.CustomFields
{
    public class CustomFieldQuery : ICustomFieldQuery
    {
        private readonly IDbConnection _dbConnection; 
        public CustomFieldQuery(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }
        public async Task<bool> FKColumnExistValidation(int Id)
        {
             var sql = "SELECT COUNT(1) FROM [AppData].[CustomField] WHERE Id = @Id AND IsDeleted = 0 AND IsActive = 1";
                var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = Id });
                return count > 0;
        }

      public async Task<(List<CustomField>, int)> GetAllCustomFieldsAsync(int pageNumber, int pageSize, string? searchTerm)
        {
            var query = $@"
                DECLARE @TotalCount INT;
                SELECT @TotalCount = COUNT(*) 
                FROM [AppData].[CustomField]
                WHERE IsDeleted = 0
                {(string.IsNullOrEmpty(searchTerm) ? "" : "AND (LabelName LIKE @Search)")};

                SELECT  
                    C.[Id],
                    C.[LabelName],
                    C.[Length],
                    C.[IsRequired],
                    C.[IsActive],
                    C.[CreatedBy],
                    C.[CreatedAt],
                    C.[CreatedByName],
                    C.[CreatedIP],
                    C.[ModifiedBy],
                    C.[ModifiedAt],
                    C.[ModifiedByName],
                    C.[ModifiedIP],
                    C.[LabelTypeId],
                    LabelType.[Id] ,
                    LabelType.[Code] ,
                    C.[DataTypeId],
                    DataType.[Id] ,
                    DataType.[Code] 
                FROM [AppData].[CustomField] C
                INNER JOIN [AppData].[MiscMaster] LabelType ON C.LabelTypeId = LabelType.Id
                INNER JOIN [AppData].[MiscMaster] DataType ON C.DataTypeId = DataType.Id
                WHERE C.IsDeleted = 0
                {(string.IsNullOrEmpty(searchTerm) ? "" : "AND (C.LabelName LIKE @Search)")}
                ORDER BY C.Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                SELECT @TotalCount AS TotalCount;
            ";

            var parameters = new
            {
                Search = $"%{searchTerm}%",
                Offset = (pageNumber - 1) * pageSize,
                PageSize = pageSize
            };

            using var multi = await _dbConnection.QueryMultipleAsync(query, parameters);

            var customFieldList = multi.Read<CustomField, UserManagement.Domain.Entities.MiscMaster, UserManagement.Domain.Entities.MiscMaster, CustomField>(
                (customField, labelType, dataType) =>
                {
                    customField.LabelType = new UserManagement.Domain.Entities.MiscMaster
                    {
                        Id = labelType.Id,
                        Code = labelType.Code
                    };

                    customField.DataType = new UserManagement.Domain.Entities.MiscMaster
                    {
                        Id = dataType.Id,
                        Code = dataType.Code
                    };

                    return customField;
                },
                splitOn: "Id,Id"
            ).ToList();

            var totalCount = multi.ReadFirst<int>();

            return (customFieldList, totalCount);
        }


         public async Task<bool> AlreadyExistsAsync(string LabelName, int? id = null)
          {
              var query = "SELECT COUNT(1) FROM [AppData].[CustomField] WHERE LabelName = @LabelName AND IsDeleted = 0";
                var parameters = new DynamicParameters(new { LabelName = LabelName });

             if (id is not null)
             {
                 query += " AND Id != @Id";
                 parameters.Add("Id", id);
             }
                var count = await _dbConnection.ExecuteScalarAsync<int>(query, parameters);
                return count > 0;
          }

        public async Task<(dynamic CustomField,IList<dynamic> CustomFieldMenu,IList<dynamic> CustomFieldUnit,IList<dynamic> CustomFieldOptionValue)> GetByIdAsync(int id)
        {
            const string query = @"SELECT Id,LabelName,DataTypeId,Length,LabelTypeId,IsRequired,IsActive
                                     FROM [AppData].[CustomField] WHERE Id = @Id AND IsDeleted = 0

                                  SELECT MenuId FROM [AppData].[CustomFieldMenu] WHERE CustomFieldId = @Id 

                                  SELECT UnitId FROM [AppData].[CustomFieldUnit] WHERE CustomFieldId = @Id 
                                  
                                  SELECT OptionFieldValue FROM [AppData].[CustomFieldOptionalValue] WHERE CustomFieldId = @Id  ";
            using var multi = await _dbConnection.QueryMultipleAsync(query, new { Id = id });
            var customfieldResult = await multi.ReadFirstOrDefaultAsync<dynamic>();
            var customFieldMenuResult = await multi.ReadAsync<dynamic>();
            var customFieldUnit = await multi.ReadAsync<dynamic>();
            var customFieldOption = await multi.ReadAsync<dynamic>();

            return (customfieldResult,customFieldMenuResult.ToList(),customFieldUnit.ToList(),customFieldOption.ToList());
        }

        public async Task<List<CustomField>> GetCustomField(string searchPattern)
        {
            
                throw new NotImplementedException();
        }

        public Task<bool> SoftDeleteValidation(int Id)
        {
            throw new NotImplementedException();
        }
         public async Task<bool> NotFoundAsync(int id)
        {
             var query = "SELECT COUNT(1) FROM [AppData].[CustomField] WHERE Id = @Id AND IsDeleted = 0";
             
                var count = await _dbConnection.ExecuteScalarAsync<int>(query, new { Id = id });
                return count > 0;
        }
    }
}