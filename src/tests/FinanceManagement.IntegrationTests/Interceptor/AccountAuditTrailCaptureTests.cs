using Dapper;
using Microsoft.Data.SqlClient;
using FinanceManagement.Application.AccountAuditTrail.Dto;
using FinanceManagement.Domain.Entities;

namespace FinanceManagement.IntegrationTests.Interceptor
{
    // US-GL02-09 AC-1 — the interceptor writes a field-level audit row in the same SaveChanges as the
    // change. AccountTypeMaster is used (an IAuditTrailed entity with no FK prerequisites and no freeze
    // trigger), keeping the test focused on the audit capture itself.
    [Collection("DatabaseCollection")]
    public sealed class AccountAuditTrailCaptureTests
    {
        private readonly DbFixture _fixture;

        public AccountAuditTrailCaptureTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private async Task<List<AccountAuditTrailDto>> ReadRowsAsync(string entityName, int entityId)
        {
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();
            var rows = await conn.QueryAsync<AccountAuditTrailDto>(@"
                SELECT Id, CompanyId, EntityName, EntityId, Action, PropertyName, OldValue, NewValue,
                       CreatedBy, CreatedByName, CreatedByRole, CreatedIP, CreatedDate
                FROM Finance.AccountAuditTrail
                WHERE EntityName=@e AND EntityId=@id
                ORDER BY CreatedDate, Id",
                new { e = entityName, id = entityId });
            return rows.ToList();
        }

        // StartCode must be unique per (CompanyId, StartCode); tests share the DB, so each passes its own.
        private static AccountTypeMaster NewType(string name, string startCode) => new()
        {
            CompanyId = 1,
            AccountTypeName = name,
            StartCode = startCode,
            AccountCodeLength = 6,
            SortOrder = 1,
            IsActive = Status.Active,
            IsDeleted = IsDelete.NotDeleted
        };

        [Fact]
        public async Task Insert_WritesInsertAuditRows_WithIdAndRole()
        {
            await _fixture.ClearAuditTrailAsync();

            int id;
            await using (var ctx = _fixture.CreateAuditingDbContext())
            {
                var entity = NewType("Assets", "2");
                ctx.AccountTypeMaster.Add(entity);
                await ctx.SaveChangesAsync();
                id = entity.Id;
            }

            id.Should().BeGreaterThan(0);
            var rows = await ReadRowsAsync("AccountTypeMaster", id);
            rows.Should().NotBeEmpty();
            rows.Should().OnlyContain(r => r.Action == "Insert");
            rows.Should().OnlyContain(r => r.CreatedByRole == "Finance Controller");
            rows.Should().Contain(r => r.PropertyName == "AccountTypeName" && r.NewValue == "Assets");
        }

        [Fact]
        public async Task Update_WritesFieldLevelRow_OldAndNew()
        {
            await _fixture.ClearAuditTrailAsync();

            int id;
            await using (var ctx = _fixture.CreateAuditingDbContext())
            {
                var entity = NewType("Liabilities", "3");
                ctx.AccountTypeMaster.Add(entity);
                await ctx.SaveChangesAsync();
                id = entity.Id;
            }

            await _fixture.ClearAuditTrailAsync(); // isolate the update rows from the insert rows

            await using (var ctx = _fixture.CreateAuditingDbContext())
            {
                var entity = await ctx.AccountTypeMaster.FirstAsync(x => x.Id == id);
                entity.AccountTypeName = "Equity";
                await ctx.SaveChangesAsync();
            }

            var rows = await ReadRowsAsync("AccountTypeMaster", id);
            rows.Should().ContainSingle();
            rows[0].Action.Should().Be("Update");
            rows[0].PropertyName.Should().Be("AccountTypeName");
            rows[0].OldValue.Should().Be("Liabilities");
            rows[0].NewValue.Should().Be("Equity");
            rows[0].CreatedByRole.Should().Be("Finance Controller");
            rows[0].CompanyId.Should().Be(1);
        }

        [Fact]
        public async Task Update_UnchangedValue_WritesNoRow()
        {
            await _fixture.ClearAuditTrailAsync();

            int id;
            await using (var ctx = _fixture.CreateAuditingDbContext())
            {
                var entity = NewType("Income", "4");
                ctx.AccountTypeMaster.Add(entity);
                await ctx.SaveChangesAsync();
                id = entity.Id;
            }

            await _fixture.ClearAuditTrailAsync();

            await using (var ctx = _fixture.CreateAuditingDbContext())
            {
                var entity = await ctx.AccountTypeMaster.FirstAsync(x => x.Id == id);
                entity.AccountTypeName = "Income"; // same value — no real change
                await ctx.SaveChangesAsync();
            }

            (await ReadRowsAsync("AccountTypeMaster", id)).Should().BeEmpty();
        }
    }
}
