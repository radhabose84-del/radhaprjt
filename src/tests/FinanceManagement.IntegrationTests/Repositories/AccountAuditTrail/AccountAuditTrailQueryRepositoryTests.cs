using Dapper;
using Microsoft.Data.SqlClient;
using FinanceManagement.Infrastructure.Repositories.AccountAuditTrail;

namespace FinanceManagement.IntegrationTests.Repositories.AccountAuditTrail
{
    [Collection("DatabaseCollection")]
    public sealed class AccountAuditTrailQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public AccountAuditTrailQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private AccountAuditTrailQueryRepository CreateRepo() =>
            new(new SqlConnection(_fixture.ConnectionString));

        private async Task SeedAsync(int companyId, string entityName, int entityId, string prop,
            string? oldV, string? newV, DateTimeOffset when, string action = "Update")
        {
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();
            await conn.ExecuteAsync(@"
INSERT INTO Finance.AccountAuditTrail
 (CompanyId, EntityName, EntityId, Action, PropertyName, OldValue, NewValue,
  CreatedBy, CreatedByName, CreatedByRole, CreatedIP, CreatedDate, Scope, ScopeKey)
VALUES
 (@CompanyId,@EntityName,@EntityId,@Action,@PropertyName,@OldValue,@NewValue,
  372,'test-user','Finance Controller','127.0.0.1',@CreatedDate,@EntityName,@ScopeKey)",
                new
                {
                    CompanyId = companyId, EntityName = entityName, EntityId = entityId, Action = action,
                    PropertyName = prop, OldValue = oldV, NewValue = newV, CreatedDate = when,
                    ScopeKey = $"Id={entityId}"
                });
        }

        private static readonly DateTimeOffset T0 = new(2026, 6, 18, 9, 0, 0, TimeSpan.Zero);

        [Fact]
        public async Task GetHistory_ReturnsRows_InChronologicalOrder()
        {
            await _fixture.ClearAuditTrailAsync();
            // Inserted out of chronological order on purpose.
            await SeedAsync(1, "GlAccountMaster", 42, "AccountName", "B", "C", T0.AddHours(2));
            await SeedAsync(1, "GlAccountMaster", 42, "Description", "A", "B", T0);
            await SeedAsync(1, "GlAccountMaster", 42, "IsTaxRelevant", "false", "true", T0.AddHours(1));

            var rows = await CreateRepo().GetHistoryAsync(1, "GlAccountMaster", 42, CancellationToken.None);

            rows.Should().HaveCount(3);
            rows.Select(r => r.PropertyName).Should()
                .ContainInOrder("Description", "IsTaxRelevant", "AccountName");
        }

        [Fact]
        public async Task GetHistory_ExcludesOtherCompany()
        {
            await _fixture.ClearAuditTrailAsync();
            await SeedAsync(1, "GlAccountMaster", 42, "Description", "A", "B", T0);
            await SeedAsync(2, "GlAccountMaster", 42, "Description", "X", "Y", T0);

            var rows = await CreateRepo().GetHistoryAsync(1, "GlAccountMaster", 42, CancellationToken.None);

            rows.Should().ContainSingle();
            rows[0].CompanyId.Should().Be(1);
        }

        [Fact]
        public async Task GetHistory_ExcludesOtherEntity()
        {
            await _fixture.ClearAuditTrailAsync();
            await SeedAsync(1, "GlAccountMaster", 42, "Description", "A", "B", T0);
            await SeedAsync(1, "AccountGroup", 9, "GroupName", "G1", "G2", T0);

            var rows = await CreateRepo().GetHistoryAsync(1, "GlAccountMaster", 42, CancellationToken.None);

            rows.Should().ContainSingle();
            rows[0].EntityName.Should().Be("GlAccountMaster");
        }

        [Fact]
        public async Task Export_CountsRowsInRange_AndFiltersByEntity()
        {
            await _fixture.ClearAuditTrailAsync();
            await SeedAsync(1, "GlAccountMaster", 42, "Description", "A", "B", T0);
            await SeedAsync(1, "AccountGroup", 9, "GroupName", "G1", "G2", T0.AddHours(1));
            await SeedAsync(1, "GlAccountMaster", 7, "AccountName", "N1", "N2", T0.AddHours(2));

            var from = T0.AddHours(-1);
            var to = T0.AddHours(5);

            var all = await CreateRepo().ExportAsync(1, from, to, null, CancellationToken.None);
            all.Should().HaveCount(3);

            var onlyAccounts = await CreateRepo().ExportAsync(1, from, to, "GlAccountMaster", CancellationToken.None);
            onlyAccounts.Should().HaveCount(2);
            onlyAccounts.Should().OnlyContain(r => r.EntityName == "GlAccountMaster");
        }

        [Fact]
        public async Task Export_ExcludesRowsOutsideDateRange()
        {
            await _fixture.ClearAuditTrailAsync();
            await SeedAsync(1, "GlAccountMaster", 42, "Description", "A", "B", T0);                 // in
            await SeedAsync(1, "GlAccountMaster", 42, "AccountName", "N1", "N2", T0.AddDays(-10));  // before
            await SeedAsync(1, "GlAccountMaster", 42, "IsTaxRelevant", "f", "t", T0.AddDays(10));   // after

            var rows = await CreateRepo().ExportAsync(
                1, T0.AddHours(-1), T0.AddHours(1), null, CancellationToken.None);

            rows.Should().ContainSingle();
            rows[0].PropertyName.Should().Be("Description");
        }
    }
}
