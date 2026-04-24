using Dapper;
using Microsoft.Data.SqlClient;
using SalesManagement.Infrastructure.Repositories.GatePass;
using SalesManagement.IntegrationTests.Common;

namespace SalesManagement.IntegrationTests.Repositories.GatePass
{
    [Collection("DatabaseCollection")]
    public sealed class DeliveryChallanGatePassProcessorTests
    {
        private readonly DbFixture _fixture;

        public DeliveryChallanGatePassProcessorTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private DeliveryChallanGatePassProcessor CreateProcessor() => new();

        private async Task<int> SeedDcAsync(int fromPlantId = 1, bool isDeleted = false, bool initialGeFlag = false)
        {
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();
            var id = await conn.ExecuteScalarAsync<int>(@"
                ALTER TABLE Sales.DeliveryChallanHeader NOCHECK CONSTRAINT ALL;
                INSERT INTO Sales.DeliveryChallanHeader
                    (DeliveryNumber, DeliveryDate, StoHeaderId,
                     FromPlantId, FromStorageLocationId, ToPlantId, ToStorageLocationId,
                     TransporterId, VehicleNumber,
                     DeliveryValue, ConsignmentValue, StatusId,
                     GEFlag, IsActive, IsDeleted,
                     CreatedBy, CreatedDate, CreatedByName, CreatedIP)
                OUTPUT INSERTED.Id
                VALUES
                    ('DC-001', SYSDATETIME(), 1,
                     @FromPlantId, 1, 2, 1,
                     1, 'TN-01-AB-1234',
                     0, 0, 1,
                     @InitialGeFlag, 1, @IsDeleted,
                     1, SYSDATETIMEOFFSET(), 'test', '127.0.0.1');
                ALTER TABLE Sales.DeliveryChallanHeader CHECK CONSTRAINT ALL;",
                new { FromPlantId = fromPlantId, IsDeleted = isDeleted, InitialGeFlag = initialGeFlag });
            return id;
        }

        private async Task<bool> GetGeFlagAsync(int id)
        {
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();
            return await conn.ExecuteScalarAsync<bool>(
                "SELECT GEFlag FROM Sales.DeliveryChallanHeader WHERE Id = @Id",
                new { Id = id });
        }

        private async Task ClearAsync() =>
            await _fixture.ClearTablesAsync(
                "Sales.StoReceiptDetail", "Sales.StoReceiptHeader",
                "Sales.DeliveryChallanDetail", "Sales.DeliveryChallanHeader");

        [Fact]
        public async Task MarkAsGatePassedAsync_Sets_GeFlag_To_True()
        {
            await ClearAsync();
            var id = await SeedDcAsync(fromPlantId: 5);

            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();
            await using var tx = (SqlTransaction)await conn.BeginTransactionAsync();

            await CreateProcessor().MarkAsGatePassedAsync(id, 5, conn, tx);

            await tx.CommitAsync();

            (await GetGeFlagAsync(id)).Should().BeTrue();
        }

        [Fact]
        public async Task MarkAsGatePassedAsync_DoesNotUpdate_When_FromPlantId_Mismatch()
        {
            await ClearAsync();
            var id = await SeedDcAsync(fromPlantId: 5);

            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();
            await using var tx = (SqlTransaction)await conn.BeginTransactionAsync();

            await CreateProcessor().MarkAsGatePassedAsync(id, 999, conn, tx);

            await tx.CommitAsync();

            (await GetGeFlagAsync(id)).Should().BeFalse();
        }

        [Fact]
        public async Task MarkAsGatePassedAsync_DoesNotUpdate_When_SoftDeleted()
        {
            await ClearAsync();
            var id = await SeedDcAsync(fromPlantId: 5, isDeleted: true);

            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();
            await using var tx = (SqlTransaction)await conn.BeginTransactionAsync();

            await CreateProcessor().MarkAsGatePassedAsync(id, 5, conn, tx);

            await tx.CommitAsync();

            (await GetGeFlagAsync(id)).Should().BeFalse();
        }

        [Fact]
        public async Task RevertGatePassAsync_Sets_GeFlag_To_False()
        {
            await ClearAsync();
            var id = await SeedDcAsync(fromPlantId: 5, initialGeFlag: true);

            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();
            await using var tx = (SqlTransaction)await conn.BeginTransactionAsync();

            await CreateProcessor().RevertGatePassAsync(id, 5, conn, tx);

            await tx.CommitAsync();

            (await GetGeFlagAsync(id)).Should().BeFalse();
        }

        [Fact]
        public void DocumentType_Matches_Stodc_TransactionType()
        {
            CreateProcessor().DocumentType.Should().Be(SalesManagement.Domain.Common.MiscEnumEntity.TransactionTypeStodc);
        }
    }
}
