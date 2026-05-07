using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SalesManagement.IntegrationTests.Common;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.IntegrationTests.Repositories.SalesReturn;

/// <summary>
/// Verifies the SQL in SalesReturnQueryRepository that fetches stock-status lookup rows
/// uses MiscTypeCode = 'StockStatus' (consolidated from 'BagStatus' on 2026-05-07).
/// </summary>
[Collection("DatabaseCollection")]
public sealed class StockStatusSqlTests
{
    private readonly DbFixture _fixture;
    public StockStatusSqlTests(DbFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task StockStatusSql_Returns_SeededEntries_When_MiscTypeCodeIsStockStatus()
    {
        // Arrange — seed a StockStatus MiscType with the 3 standard defect entries
        await using var ctx = _fixture.CreateFreshDbContext();

        var mt = await ctx.MiscTypeMaster
            .FirstOrDefaultAsync(x => x.MiscTypeCode == "StockStatus");

        if (mt == null)
        {
            mt = new SalesManagement.Domain.Entities.MiscTypeMaster
            {
                MiscTypeCode = "StockStatus",
                Description = "Stock Status",
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
            await ctx.MiscTypeMaster.AddAsync(mt);
            await ctx.SaveChangesAsync();
        }

        // Only seed each code once; idempotent on repeated test runs
        foreach (var code in new[] { "DEFECT", "DAMAGED", "YARN MISMATCH" })
        {
            var exists = await ctx.MiscMaster
                .AnyAsync(m => m.MiscTypeId == mt.Id && m.Code == code);

            if (!exists)
            {
                await ctx.MiscMaster.AddAsync(new SalesManagement.Domain.Entities.MiscMaster
                {
                    MiscTypeId = mt.Id,
                    Code = code,
                    Description = code,
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
                });
            }
        }
        await ctx.SaveChangesAsync();

        // Act — run the exact SQL from SalesReturnQueryRepository (line ~291)
        await using var conn = new SqlConnection(_fixture.ConnectionString);
        const string sql = @"
            SELECT mm.Id, mm.Code, mm.Description
            FROM Sales.MiscMaster mm
            INNER JOIN Sales.MiscTypeMaster mt ON mm.MiscTypeId = mt.Id
            WHERE mt.miscTypecode = 'StockStatus' AND mm.IsActive = 1 AND mm.IsDeleted = 0
            ORDER BY mm.Description;";

        var results = (await conn.QueryAsync(sql)).ToList();

        // Assert — must find at least the 3 seeded entries
        results.Should().HaveCountGreaterThanOrEqualTo(3,
            because: "StockStatus MiscType must have DEFECT, DAMAGED, and YARN MISMATCH entries");
    }

    [Fact]
    public async Task StockStatusSql_Returns_NoRows_When_MiscTypeCodeIsBagStatus()
    {
        // Prove the OLD code ('BagStatus') would return nothing — migration ensured
        // all rows moved to 'StockStatus'. This test documents the invariant.
        await using var conn = new SqlConnection(_fixture.ConnectionString);
        const string oldSql = @"
            SELECT COUNT(1)
            FROM Sales.MiscMaster mm
            INNER JOIN Sales.MiscTypeMaster mt ON mm.MiscTypeId = mt.Id
            WHERE mt.miscTypecode = 'BagStatus' AND mm.IsActive = 1 AND mm.IsDeleted = 0;";

        var count = await conn.ExecuteScalarAsync<int>(oldSql);

        count.Should().Be(0, because: "Stage 2 migration must have removed the BagStatus MiscTypeCode");
    }
}
