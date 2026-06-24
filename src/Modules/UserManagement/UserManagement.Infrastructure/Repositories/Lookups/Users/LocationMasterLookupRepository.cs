using System.Data;
using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Users;
using Dapper;
using UserManagement.Application.Common.Interfaces.ILocation;
using static UserManagement.Domain.Enums.Common.Enums;

namespace UserManagement.Infrastructure.Repositories.Lookups.Users
{
    internal sealed class LocationMasterLookupRepository : ILocationMasterLookup
    {
        private readonly IDbConnection _dbConnection;
        private readonly ILocationQueryRepository _locationQueryRepository;
        private readonly ILocationCommandRepository _locationCommandRepository;

        public LocationMasterLookupRepository(
            IDbConnection dbConnection,
            ILocationQueryRepository locationQueryRepository,
            ILocationCommandRepository locationCommandRepository)
        {
            _dbConnection = dbConnection;
            _locationQueryRepository = locationQueryRepository;
            _locationCommandRepository = locationCommandRepository;
        }

        public async Task<LocationMasterLookupDto?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            const string sql = @"
                SELECT L.Id, L.Code, L.LocationName
                FROM [AppData].[Location] L
                WHERE L.Id = @Id AND L.IsActive = 1 AND L.IsDeleted = 0;";

            var cmd = new CommandDefinition(sql, new { Id = id }, cancellationToken: ct);
            return await _dbConnection.QueryFirstOrDefaultAsync<LocationMasterLookupDto>(cmd);
        }

        public async Task<IReadOnlyList<LocationMasterLookupDto>> GetByIdsAsync(IEnumerable<int> ids, CancellationToken ct = default)
        {
            var idList = (ids ?? Array.Empty<int>()).Where(x => x > 0).Distinct().ToArray();
            if (idList.Length == 0)
                return Array.Empty<LocationMasterLookupDto>();

            const string sql = @"
                SELECT L.Id, L.Code, L.LocationName
                FROM [AppData].[Location] L
                WHERE L.Id IN @Ids AND L.IsDeleted = 0;";

            var cmd = new CommandDefinition(sql, new { Ids = idList }, cancellationToken: ct);
            var rows = await _dbConnection.QueryAsync<LocationMasterLookupDto>(cmd);
            return rows.ToList();
        }

        public async Task<int> GetOrCreateByNameAsync(string locationName, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(locationName))
                return 0;

            static string Normalize(string s) =>
                string.IsNullOrWhiteSpace(s) ? string.Empty : s.Trim().ToLower().Replace(" ", "");

            var normalized = Normalize(locationName);

            // Reuse: find an existing, non-deleted location whose normalized name matches.
            var matches = await _locationQueryRepository.GetAllLocationAsync(locationName);
            var existing = matches.FirstOrDefault(l =>
                Normalize(l.LocationName ?? string.Empty) == normalized &&
                l.IsDeleted == IsDelete.NotDeleted);

            if (existing != null)
                return existing.Id;

            // Insert: derive a unique alphanumeric Code from the name (Code is unique on the master).
            var baseCode = new string(locationName.Where(char.IsLetterOrDigit).ToArray()).ToUpperInvariant();
            if (baseCode.Length == 0)
                baseCode = "LOC";
            baseCode = baseCode[..Math.Min(15, baseCode.Length)];

            var code = baseCode;
            var suffix = 1;
            while (await _locationQueryRepository.AlreadyExistsAsync(code))
            {
                code = baseCode + suffix;
                suffix++;
            }

            var created = new UserManagement.Domain.Entities.Location
            {
                LocationName = locationName.TrimEnd(),
                Code = code,
                IsActive = Status.Active
            };

            return await _locationCommandRepository.CreateAsync(created);
        }
    }
}
