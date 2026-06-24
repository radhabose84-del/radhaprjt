using Dapper;
using Microsoft.Data.SqlClient;

namespace FinanceManagement.IntegrationTests.Immutability
{
    // US-GL02-09 AC-2 — the immutability trigger rejects UPDATE and DELETE on the audit table.
    // The trigger lives in the migration (raw SQL), which EnsureCreated does not apply, so the test
    // installs it for the duration of the test and removes it afterwards.
    [Collection("DatabaseCollection")]
    public sealed class AccountAuditTrailImmutabilityTests
    {
        private readonly DbFixture _fixture;

        public AccountAuditTrailImmutabilityTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private async Task<int> SeedOneRowAsync()
        {
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();
            return await conn.ExecuteScalarAsync<int>(@"
INSERT INTO Finance.AccountAuditTrail
 (CompanyId, EntityName, EntityId, Action, PropertyName, OldValue, NewValue,
  CreatedBy, CreatedByName, CreatedByRole, CreatedIP, CreatedDate, Scope, ScopeKey)
VALUES
 (1,'GlAccountMaster',42,'Update','Description','A','B',
  372,'test-user','Finance Controller','127.0.0.1', SYSDATETIMEOFFSET(), 'GlAccountMaster','Id=42');
SELECT CAST(SCOPE_IDENTITY() AS INT);");
        }

        [Fact]
        public async Task Update_And_Delete_AreRejected_ByTrigger()
        {
            await _fixture.ClearAuditTrailAsync();
            await _fixture.InstallImmutabilityTriggerAsync();
            try
            {
                await SeedOneRowAsync();   // INSERT is allowed (trigger is INSTEAD OF UPDATE/DELETE)

                await using var conn = new SqlConnection(_fixture.ConnectionString);
                await conn.OpenAsync();

                Func<Task> update = () => conn.ExecuteAsync(
                    "UPDATE Finance.AccountAuditTrail SET NewValue='HACK' WHERE EntityId=42;");
                (await update.Should().ThrowAsync<SqlException>())
                    .Which.Message.Should().Contain("AUDIT_TRAIL_IMMUTABLE");

                Func<Task> delete = () => conn.ExecuteAsync(
                    "DELETE FROM Finance.AccountAuditTrail WHERE EntityId=42;");
                (await delete.Should().ThrowAsync<SqlException>())
                    .Which.Message.Should().Contain("AUDIT_TRAIL_IMMUTABLE");

                // The row is still intact and unmodified.
                var newValue = await conn.ExecuteScalarAsync<string>(
                    "SELECT NewValue FROM Finance.AccountAuditTrail WHERE EntityId=42;");
                newValue.Should().Be("B");
            }
            finally
            {
                // Trigger blocks DELETE — drop it before clearing.
                await _fixture.DropImmutabilityTriggerAsync();
                await _fixture.ClearAuditTrailAsync();
            }
        }
    }
}
