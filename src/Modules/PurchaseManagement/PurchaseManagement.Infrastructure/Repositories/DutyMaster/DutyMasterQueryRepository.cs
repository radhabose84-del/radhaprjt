using System.Data;
using PurchaseManagement.Application.Common.Interfaces.IPurchase.DutyMaster;
using PurchaseManagement.Application.DutyMaster;
using Dapper;
using Microsoft.EntityFrameworkCore;
using PurchaseManagement.Infrastructure.Data;

namespace PurchaseManagement.Infrastructure.Repositories.DutyMaster
{
    public class DutyMasterQueryRepository : IDutyMasterQueryRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly IDbConnection _conn;

        public DutyMasterQueryRepository(ApplicationDbContext db, IDbConnection conn)
        {
            _db = db;
            _conn = conn;
        }

        public async Task<(IReadOnlyList<PurchaseManagement.Domain.Entities.DutyMaster> Items, int Total)> GetAllAsync(int page, int size, string? search, CancellationToken ct)
        {
            page = page <= 0 ? 1 : page;
            size = size <= 0 ? 20 : size;

            var q = _db.Set<PurchaseManagement.Domain.Entities.DutyMaster>().AsNoTracking();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToUpper();
                q = q.Where(x =>
                    x.DutyCode.ToUpper().Contains(s) ||
                    x.TariffNumber.ToUpper().Contains(s) ||
                    (x.HsnCode != null && x.HsnCode.ToUpper().Contains(s)) ||
                    (x.NotificationNumber != null && x.NotificationNumber.ToUpper().Contains(s)));
            }

            var total = await q.CountAsync(ct);
            var items = await q
                .OrderByDescending(x => x.Id)
                .Skip((page - 1) * size)
                .Take(size)
                .ToListAsync(ct);

            return (items, total);
        }

        public Task<PurchaseManagement.Domain.Entities.DutyMaster?> GetByIdAsync(int id, CancellationToken ct)
            => _db.Set<PurchaseManagement.Domain.Entities.DutyMaster>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);

        public Task<bool> ExistsAsync(string dutyCode, string tariffNumber, DateTimeOffset effectiveFrom, CancellationToken ct)
            => _db.Set<PurchaseManagement.Domain.Entities.DutyMaster>().AnyAsync(x =>
                    x.DutyCode == dutyCode &&
                    x.TariffNumber == tariffNumber &&
                    x.EffectiveFrom == effectiveFrom, ct);

        public async Task<IReadOnlyList<DutyMasterAutocompleteDto>> GetAutocompleteAsync(string? term, CancellationToken ct)
        {
            term = (term ?? string.Empty).Trim();

            // Global filter should already exclude IsDeleted = true
            IQueryable<PurchaseManagement.Domain.Entities.DutyMaster> q = _db.Set<PurchaseManagement.Domain.Entities.DutyMaster>().AsNoTracking();

            if (!string.IsNullOrWhiteSpace(term))
            {
                var t = term.ToUpper();
                q = q.Where(x =>
                    x.DutyCode.ToUpper().Contains(t) ||
                    (x.Description != null && x.Description.ToUpper().Contains(t)) ||
                    x.TariffNumber.ToUpper().Contains(t) ||
                    (x.HsnCode != null && x.HsnCode.ToUpper().Contains(t)));
            }

            var data = await q
                .OrderBy(x => x.DutyCode)
                .ThenBy(x => x.TariffNumber)
                .Select(x => new DutyMasterAutocompleteDto
                {
                    Id = x.Id,
                    DutyCode = x.DutyCode,
                    Description = x.Description,
                    TariffNumber = x.TariffNumber,
                    HsnCode = x.HsnCode
                })
                .ToListAsync(ct);

            return data;
        }
        public async Task<string> GenerateDutyCodeAsync(CancellationToken ct)
        {
            const string prefix = "DUT";
            const int width = 3;

            const string sql = @"
            SELECT ISNULL(MAX(TRY_CONVERT(int, RIGHT(DutyCode, @width))), 0)
            FROM Purchase.DutyMaster WITH (NOLOCK)
            WHERE DutyCode LIKE @prefix + '-%';";


            var maxNum = await _conn.ExecuteScalarAsync<int>(
                new CommandDefinition(sql, new { prefix, width }, cancellationToken: ct));

            var next = maxNum + 1;
            return $"{prefix}-{next.ToString().PadLeft(width, '0')}";
        }
        public async Task<IReadOnlyList<DutyMasterReadDto>> GetByTariffOrHsnAsync(
      IEnumerable<string> tariffNumbers,
      IEnumerable<string> hsnCodes,
      DateTimeOffset onDate,
      CancellationToken ct = default)
        {
            var tariffs = (tariffNumbers ?? Array.Empty<string>())
                .Where(s => !string.IsNullOrWhiteSpace(s)).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
            var hsns = (hsnCodes ?? Array.Empty<string>())
                .Where(s => !string.IsNullOrWhiteSpace(s)).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();

            if (tariffs.Length == 0 && hsns.Length == 0)
                return Array.Empty<DutyMasterReadDto>();

            const string sql = @"
            ;WITH d AS
            (
                SELECT
                    dm.*,
                    ROW_NUMBER() OVER(
                        PARTITION BY ISNULL(dm.TariffNumber, ''), ISNULL(dm.HsnCode, '')
                        ORDER BY dm.EffectiveFrom DESC
                    ) AS rn
                FROM Purchase.DutyMaster dm
                WHERE dm.IsActive = 1
                AND dm.IsDeleted = 0
                AND dm.EffectiveFrom <= @OnDate
                AND (dm.EffectiveTo IS NULL OR dm.EffectiveTo >= @OnDate)
                AND (
                        ( @HasTariffs = 1 AND dm.TariffNumber IN @Tariffs )
                    OR ( @HasHsns    = 1 AND dm.HsnCode     IN @Hsns    )
                )
            )
            SELECT *
            FROM d
            WHERE rn = 1;";

            var rows = await _conn.QueryAsync<DutyMasterReadDto>(sql, new
            {
                OnDate = onDate,
                Tariffs = tariffs,
                Hsns = hsns,
                HasTariffs = tariffs.Length > 0 ? 1 : 0,
                HasHsns = hsns.Length > 0 ? 1 : 0
            });

            return rows.ToList();
        }
            
    }
}
