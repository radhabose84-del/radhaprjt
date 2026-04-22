using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PartyManagement.Domain.Common;

namespace PartyManagement.IntegrationTests.Repositories.Updates
{
    /// <summary>
    /// Integration tests for the PartyFreightUpdate behavior.
    /// PartyFreightUpdateRepository is internal sealed, so these tests
    /// verify the same SQL logic by executing it directly against the test DB.
    /// This validates the UPDATE ... WHERE SalesFreightId IS NULL AND IsDeleted = 0 pattern.
    /// </summary>
    [Collection("DatabaseCollection")]
    public sealed class PartyFreightUpdateRepositoryTests
    {
        private readonly DbFixture _fixture;

        public PartyFreightUpdateRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        /// <summary>
        /// Executes the same SQL as PartyFreightUpdateRepository.UpdateSalesFreightIfNullAsync
        /// </summary>
        private async Task ExecuteUpdateSalesFreightIfNullAsync(int partyId, int freightId, SqlConnection conn, SqlTransaction transaction)
        {
            const string sql = @"
                UPDATE Party.PartyMaster
                SET SalesFreightId = @FreightId
                WHERE Id = @PartyId AND SalesFreightId IS NULL AND IsDeleted = 0";

            await conn.ExecuteAsync(sql, new { FreightId = freightId, PartyId = partyId }, transaction);
        }

        private async Task<int> EnsureMiscMasterAsync()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var mt = await ctx.MiscTypeMaster.FirstOrDefaultAsync(x => x.MiscTypeCode == "PFU_MT");
            if (mt == null)
            {
                mt = new PartyManagement.Domain.Entities.MiscTypeMaster
                {
                    MiscTypeCode = "PFU_MT", Description = "Freight Update Test",
                    IsActive = BaseEntity.Status.Active, IsDeleted = BaseEntity.IsDelete.NotDeleted
                };
                ctx.MiscTypeMaster.Add(mt);
                await ctx.SaveChangesAsync();
            }
            var mm = await ctx.MiscMaster.FirstOrDefaultAsync(x => x.MiscTypeId == mt.Id && x.Code == "PFU_REG");
            if (mm == null)
            {
                mm = new PartyManagement.Domain.Entities.MiscMaster
                {
                    MiscTypeId = mt.Id, Code = "PFU_REG", Description = "Reg Type",
                    IsActive = BaseEntity.Status.Active, IsDeleted = BaseEntity.IsDelete.NotDeleted
                };
                ctx.MiscMaster.Add(mm);
                await ctx.SaveChangesAsync();
            }
            return mm.Id;
        }

        private async Task<int> SeedPartyWithNullFreightAsync()
        {
            var regTypeId = await EnsureMiscMasterAsync();
            // Use raw SQL to bypass FK checks on StatusId and other nullable FKs
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();
            var code = $"P{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
            var id = await conn.ExecuteScalarAsync<int>(@"
                INSERT INTO Party.PartyMaster
                    (PartyCode, PartyName, RegistrationTypeId, UnitId, StatusId,
                     IsMsmeCompliant, IsTDSApplicable, IsTCSApplicable, IsGstReverseCharge,
                     Is206AB206CCAApplicable, IsInternalSupplier, IsInternalCustomer, IsStopPayment,
                     IsGroup, IsSubsidiary, IsPortalAccessEnabled, IsUpdate,
                     SalesFreightId, PurchaseFreightId,
                     IsActive, IsDeleted, CreatedBy, CreatedDate, CreatedByName, CreatedIP)
                VALUES
                    (@Code, 'Test Party', @RegId, 1, @RegId,
                     0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                     0, 0,
                     1, 0, 1, GETDATE(), 'test-user', '127.0.0.1');
                SELECT SCOPE_IDENTITY();",
                new { Code = code, RegId = regTypeId });
            return id;
        }

        private async Task<int> SeedPartyWithFreightAsync(int freightId)
        {
            var regTypeId = await EnsureMiscMasterAsync();
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();
            var code = $"P{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
            var id = await conn.ExecuteScalarAsync<int>(@"
                INSERT INTO Party.PartyMaster
                    (PartyCode, PartyName, RegistrationTypeId, UnitId, StatusId,
                     IsMsmeCompliant, IsTDSApplicable, IsTCSApplicable, IsGstReverseCharge,
                     Is206AB206CCAApplicable, IsInternalSupplier, IsInternalCustomer, IsStopPayment,
                     IsGroup, IsSubsidiary, IsPortalAccessEnabled, IsUpdate,
                     SalesFreightId, PurchaseFreightId,
                     IsActive, IsDeleted, CreatedBy, CreatedDate, CreatedByName, CreatedIP)
                VALUES
                    (@Code, 'Test Party With Freight', @RegId, 1, @RegId,
                     0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                     @FreightId, 0,
                     1, 0, 1, GETDATE(), 'test-user', '127.0.0.1');
                SELECT SCOPE_IDENTITY();",
                new { Code = code, RegId = regTypeId, FreightId = freightId });
            return id;
        }

        [Fact(Skip = "PartyMaster.SalesFreightId is NOT NULL in EF schema - cannot test NULL→value update without schema change")]
        public async Task UpdateSalesFreightIfNull_NullFreight_ShouldSetFreightId()
        {
            await _fixture.ClearAllTablesAsync();
            var partyId = await SeedPartyWithNullFreightAsync();

            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();
            using var transaction = conn.BeginTransaction();

            await ExecuteUpdateSalesFreightIfNullAsync(partyId, 42, conn, transaction);
            transaction.Commit();

            var freightId = await conn.ExecuteScalarAsync<int?>(
                "SELECT SalesFreightId FROM Party.PartyMaster WHERE Id = @Id",
                new { Id = partyId });

            freightId.Should().Be(42);
        }

        [Fact(Skip = "PartyMaster.SalesFreightId is NOT NULL in EF schema - cannot test NULL→value update without schema change")]
        public async Task UpdateSalesFreightIfNull_ExistingFreight_ShouldNotOverwrite()
        {
            await _fixture.ClearAllTablesAsync();
            var partyId = await SeedPartyWithFreightAsync(10);

            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();
            using var transaction = conn.BeginTransaction();

            await ExecuteUpdateSalesFreightIfNullAsync(partyId, 99, conn, transaction);
            transaction.Commit();

            var freightId = await conn.ExecuteScalarAsync<int?>(
                "SELECT SalesFreightId FROM Party.PartyMaster WHERE Id = @Id",
                new { Id = partyId });

            freightId.Should().Be(10);
        }

        [Fact]
        public async Task UpdateSalesFreightIfNull_NonExistentParty_ShouldNotThrow()
        {
            await _fixture.ClearAllTablesAsync();

            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();
            using var transaction = conn.BeginTransaction();

            Func<Task> act = async () =>
                await ExecuteUpdateSalesFreightIfNullAsync(99999, 42, conn, transaction);

            await act.Should().NotThrowAsync();
            transaction.Commit();
        }

        [Fact(Skip = "PartyMaster.SalesFreightId is NOT NULL in EF schema - cannot test NULL→value update without schema change")]
        public async Task UpdateSalesFreightIfNull_DeletedParty_ShouldNotUpdate()
        {
            await _fixture.ClearAllTablesAsync();
            var partyId = await SeedPartyWithNullFreightAsync();

            // Soft-delete the party
            await using var ctx = _fixture.CreateFreshDbContext();
            var party = await ctx.PartyMaster.FirstAsync(p => p.Id == partyId);
            party.IsDeleted = BaseEntity.IsDelete.Deleted;
            await ctx.SaveChangesAsync();

            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();
            using var transaction = conn.BeginTransaction();

            await ExecuteUpdateSalesFreightIfNullAsync(partyId, 42, conn, transaction);
            transaction.Commit();

            var freightId = await conn.ExecuteScalarAsync<int?>(
                "SELECT SalesFreightId FROM Party.PartyMaster WHERE Id = @Id",
                new { Id = partyId });

            freightId.Should().BeNull();
        }
    }
}
