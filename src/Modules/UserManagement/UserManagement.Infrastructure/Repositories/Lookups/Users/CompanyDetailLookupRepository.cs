using System.Data;
using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Users;
using Dapper;

namespace UserManagement.Infrastructure.Repositories.Lookups.Users
{
    internal sealed class CompanyDetailLookupRepository : ICompanyDetailLookup
    {
        private readonly IDbConnection _dbConnection;

        public CompanyDetailLookupRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<CompanyDetailLookupDto?> GetByUnitIdAsync(int unitId, CancellationToken ct = default)
        {
            // c.Logo holds the stored logo (full path / file name); it is post-processed below
            // into a public URL using the 'CompanyLogoPath' base from AppData.MiscTypeMaster.
            const string sql = @"
                SELECT c.Id AS CompanyId, c.CompanyName, c.LegalName,
                    c.GstNumber, c.PanNumber, c.Website,
                    ca.AddressLine1, ca.AddressLine2,
                    ca.CityId, ca.StateId, ca.PinCode,
                    ca.Phone, cc.Email,
                    c.Logo AS LogoUrl
                FROM AppData.Unit u
                INNER JOIN AppData.Company c ON u.CompanyId = c.Id AND c.IsDeleted = 0
                LEFT JOIN AppData.CompanyAddress ca ON ca.CompanyId = c.Id
                LEFT JOIN AppData.CompanyContact cc ON cc.CompanyId = c.Id
                WHERE u.Id = @UnitId AND u.IsDeleted = 0";

            var dto = await _dbConnection.QueryFirstOrDefaultAsync<CompanyDetailLookupDto>(
                new CommandDefinition(sql, new { UnitId = unitId }, cancellationToken: ct));

            if (dto != null)
            {
                const string baseSql = @"
                    SELECT TOP 1 Description
                    FROM AppData.MiscTypeMaster
                    WHERE MiscTypeCode = 'CompanyLogoPath' AND IsActive = 1 AND IsDeleted = 0;";

                var baseUrl = await _dbConnection.QueryFirstOrDefaultAsync<string?>(
                    new CommandDefinition(baseSql, cancellationToken: ct));

                dto.LogoUrl = BuildLogoUrl(dto.LogoUrl, baseUrl);
            }

            return dto;
        }

        // Logos are stored flat under Resources/AllFiles (no company/unit nesting), and existing
        // rows hold absolute paths from older publishes — so we keep only the file name and rebuild
        // the URL from the configured base. Returns null when either the file or base is missing.
        private static string? BuildLogoUrl(string? logo, string? baseUrl)
        {
            if (string.IsNullOrWhiteSpace(logo) || string.IsNullOrWhiteSpace(baseUrl))
                return null;

            var fileName = Path.GetFileName(logo.Replace('\\', '/'));
            if (string.IsNullOrWhiteSpace(fileName))
                return null;

            return $"{baseUrl.TrimEnd('/')}/{fileName}";
        }
    }
}
