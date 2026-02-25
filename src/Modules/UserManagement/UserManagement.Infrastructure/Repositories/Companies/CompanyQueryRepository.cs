#nullable disable
using UserManagement.Application.Common.Interfaces.ICompany;
using UserManagement.Application.Common.Interfaces;
using System.Data;
using Dapper;
using UserManagement.Domain.Entities;

namespace UserManagement.Infrastructure.Repositories.Companies
{
    public class CompanyQueryRepository : ICompanyQueryRepository
    {
        private readonly IDbConnection _dbConnection;
        private readonly IIPAddressService _ipAddressService;
        public CompanyQueryRepository(IDbConnection dbConnection, IIPAddressService ipAddressService)
        {
            _dbConnection = dbConnection;
            _ipAddressService = ipAddressService;
        }

        public async Task<(List<Company>, int)> GetAllCompaniesAsync(int PageNumber, int PageSize, string SearchTerm)
        {
            var entityId = _ipAddressService.GetEntityId();
            var query = $$"""
            DECLARE @TotalCount INT;
             SELECT @TotalCount = COUNT(*) 
               FROM AppData.Company C
              WHERE C.IsDeleted = 0
            {{(string.IsNullOrEmpty(SearchTerm) ? "" : "AND (C.CompanyName LIKE @Search OR C.LegalName LIKE @Search)")}};

                SELECT 
            C.Id, 
            C.CompanyName, 
            C.LegalName,
            C.GstNumber,
            C.TIN,
            C.TAN,
            C.CSTNo,
            C.YearOfEstablishment,
            C.Website,
            C.Logo,
            C.EntityId, 
            C.IsActive,
            C.CreatedByName,
            C.CreatedAt ,
            C.CreatedBy,
            C.CreatedIP,
            C.ModifiedByName,
            C.ModifiedAt,
            C.ModifiedBy,
            C.ModifiedIP,
            c.PanNumber
             FROM AppData.Company C
              WHERE C.IsDeleted = 0 AND EntityId=@EntityId
                {{(string.IsNullOrEmpty(SearchTerm) ? "" : "AND (C.CompanyName LIKE @Search OR C.LegalName LIKE @Search)")}}
              ORDER BY C.Id DESC
              OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

             SELECT @TotalCount AS TotalCount;
            """;


            var parameters = new
            {
                Search = $"%{SearchTerm}%",
                Offset = (PageNumber - 1) * PageSize,
                PageSize,
                EntityId = entityId
            };
            var company = await _dbConnection.QueryMultipleAsync(query, parameters);
            var companies = (await company.ReadAsync<Company>()).ToList();
            int totalCount = (await company.ReadFirstAsync<int>());
            return (companies, totalCount);

        }
        public async Task<Company> GetByCompanynameAsync(string name, int? id = null)
        {

            var query = """
                 SELECT * FROM AppData.Company 
                 WHERE CompanyName = @Name AND IsDeleted = 0
                 """;

            var parameters = new DynamicParameters(new { Name = name });

            if (id is not null)
            {
                query += " AND Id != @Id";
                parameters.Add("Id", id);
            }

            return await _dbConnection.QueryFirstOrDefaultAsync<Company>(query, parameters);
        }

        public async Task<Company> GetByIdAsync(int id)
        {
            const string query = @"
             SELECT 
                C.Id, 
            C.CompanyName, 
            C.LegalName,
            C.GstNumber,
            C.TIN,
            C.TAN,
            C.CSTNo,
            C.YearOfEstablishment,
            C.Website,
            C.Logo,
            C.EntityId, 
            C.PanNumber,
            C.IsActive,
            A.AddressLine1,
            A.AddressLine2,
            A.PinCode,
            A.CountryId,
            A.StateId,
            A.CityId,
            A.Phone,
            A.AlternatePhone,
            B.Name,
                 B.Designation,
                 B.Email,
                 B.Phone ,
                 B.Remark As Remarks 
             FROM AppData.Company C
             LEFT JOIN AppData.CompanyAddress A ON A.CompanyId = C.Id
             LEFT JOIN AppData.CompanyContact B ON B.CompanyId = C.Id
             WHERE C.Id = @id AND C.IsDeleted = 0";
            var companyResponse = await _dbConnection.QueryAsync<Company, CompanyAddress, CompanyContact, Company>(query,
            (company, companyAddress, companyContact) =>
            {
                company.CompanyAddress = companyAddress;
                company.CompanyContact = companyContact;
                return company;
            },
            new { id },
            splitOn: "AddressLine1,Name");

            return companyResponse.FirstOrDefault();

        }

        public async Task<List<Company>> GetCompany(int userId, string searchPattern = null)
        {
            var entityId = _ipAddressService.GetEntityId();

            const string query = @"
                SELECT 
                C.Id, 
                C.CompanyName
            FROM AppData.Company C
            Inner JOIN [AppSecurity].[UserCompany] UC ON UC.CompanyId = C.Id 
            where IsDeleted = 0 and CompanyName like @SearchPattern and UC.UserId = @UserId  and UC.IsActive = 1 AND C.EntityId=@EntityId";


            var result = await _dbConnection.QueryAsync<Company>(query, new
            {
                SearchPattern = $"%{searchPattern}%",
                UserId = userId,
                EntityId = entityId

            });
            return result.ToList();
        }
        public async Task<bool> CompanyExistsAsync(string companyName)
        {
            var sql = "SELECT COUNT(1) FROM AppData.Company WHERE CompanyName = @CompanyName";
            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { CompanyName = companyName });
            return count > 0;
        }
        public async Task<bool> PanNumberExistsAsync(string panNumber)
        {
            var sql = "SELECT COUNT(1) FROM AppData.Company WHERE PanNumber = @PanNumber";
            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { PanNumber = panNumber });
            return count > 0;
        }
        public async Task<bool> SoftDeleteValidation(int Id)
        {
            const string query = @"
                           SELECT 1 
                           FROM [AppData].[CompanySetting] 
                           WHERE CompanyId = @Id AND IsDeleted = 0;
                    
                           SELECT 1 
                           FROM [AppData].[Division]
                           WHERE CompanyId = @Id AND IsDeleted = 0;
                           
                           SELECT 1 
                           FROM [AppData].[Unit]
                           WHERE CompanyId = @Id AND IsDeleted = 0;";

            using var multi = await _dbConnection.QueryMultipleAsync(query, new { Id = Id });

            var companySettingExists = await multi.ReadFirstOrDefaultAsync<int?>();
            var divisionExists = await multi.ReadFirstOrDefaultAsync<int?>();
            var unitExists = await multi.ReadFirstOrDefaultAsync<int?>();

            return companySettingExists.HasValue || divisionExists.HasValue || unitExists.HasValue;
        }
        public async Task<bool> FKColumnExistValidation(int companyId)
        {
            var sql = "SELECT COUNT(1) FROM BannariERP.AppData.Company WHERE Id = @Id AND IsDeleted = 0 AND IsActive = 1";
            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = companyId });
            return count > 0;
        }
        public async Task<List<Company>> GetCompany_SuperAdmin(string searchPattern = null)
        {
            var entityId = _ipAddressService.GetEntityId();

            const string query = @"
                SELECT 
                C.Id, 
                C.CompanyName
            FROM BannariERP.AppData.Company C
            where IsDeleted = 0 and CompanyName like @SearchPattern  AND C.EntityId=@EntityId";


            var result = await _dbConnection.QueryAsync<Company>(query, new
            {
                SearchPattern = $"%{searchPattern}%",
                EntityId = entityId

            });
            return result.ToList();
        }

        public async Task<bool> IsCompanyUsedByAnyUserAsync(int companyId)
        {

        const string sql = @"
        SELECT COUNT(1)
        FROM BannariERP.AppSecurity.UserCompany
        WHERE CompanyId = @CompanyId
        AND IsActive = 1;";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { CompanyId = companyId });
            return count > 0;
        }
    }
}