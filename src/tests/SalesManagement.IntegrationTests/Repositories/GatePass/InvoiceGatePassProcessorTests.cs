using Dapper;
using Microsoft.Data.SqlClient;
using SalesManagement.Infrastructure.Repositories.GatePass;
using SalesManagement.IntegrationTests.Common;

namespace SalesManagement.IntegrationTests.Repositories.GatePass
{
    [Collection("DatabaseCollection")]
    public sealed class InvoiceGatePassProcessorTests
    {
        private readonly DbFixture _fixture;

        public InvoiceGatePassProcessorTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private InvoiceGatePassProcessor CreateProcessor() => new();

        private async Task<int> SeedInvoiceAsync(int unitId = 1, bool isDeleted = false, bool initialGeFlag = false)
        {
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();
            var id = await conn.ExecuteScalarAsync<int>(@"
                ALTER TABLE Sales.InvoiceHeader NOCHECK CONSTRAINT ALL;
                INSERT INTO Sales.InvoiceHeader
                    (InvoiceNo, InvoiceDate, DispatchAdviceId, PartyId, UnitId, FinancialYearId,
                     TotalBags, TotalWeight, TaxableValue, TotalDiscount, TotalFreight, TotalCommission, Insurance,
                     HandlingCharge, TotalCharity, OtherCharges, CGST, SGST, IGST, TaxAmount,
                     TCSPercentage, TCS, RoundOff, InvoiceAmountBeforeTCS, InvoiceAmount,
                     GEFlag, IsActive, IsDeleted,
                     CreatedBy, CreatedDate, CreatedByName, CreatedIP)
                OUTPUT INSERTED.Id
                VALUES
                    ('INV-001', SYSDATETIME(), 1, 1, @UnitId, 1,
                     0, 0, 0, 0, 0, 0, 0,
                     0, 0, 0, 0, 0, 0, 0,
                     0, 0, 0, 0, 0,
                     @InitialGeFlag, 1, @IsDeleted,
                     1, SYSDATETIMEOFFSET(), 'test', '127.0.0.1');
                ALTER TABLE Sales.InvoiceHeader CHECK CONSTRAINT ALL;",
                new { UnitId = unitId, IsDeleted = isDeleted, InitialGeFlag = initialGeFlag });
            return id;
        }

        private async Task<bool> GetGeFlagAsync(int id)
        {
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();
            return await conn.ExecuteScalarAsync<bool>(
                "SELECT GEFlag FROM Sales.InvoiceHeader WHERE Id = @Id",
                new { Id = id });
        }

        private async Task ClearAsync() =>
            await _fixture.ClearTablesAsync("Sales.InvoiceDetail", "Sales.InvoiceHeader");

        [Fact]
        public async Task MarkAsGatePassedAsync_Sets_GeFlag_To_True()
        {
            await ClearAsync();
            var id = await SeedInvoiceAsync(unitId: 5);

            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();
            await using var tx = (SqlTransaction)await conn.BeginTransactionAsync();

            await CreateProcessor().MarkAsGatePassedAsync(id, 5, conn, tx);

            await tx.CommitAsync();

            (await GetGeFlagAsync(id)).Should().BeTrue();
        }

        [Fact]
        public async Task MarkAsGatePassedAsync_DoesNotUpdate_When_UnitId_Mismatch()
        {
            await ClearAsync();
            var id = await SeedInvoiceAsync(unitId: 5);

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
            var id = await SeedInvoiceAsync(unitId: 5, isDeleted: true);

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
            var id = await SeedInvoiceAsync(unitId: 5, initialGeFlag: true);

            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();
            await using var tx = (SqlTransaction)await conn.BeginTransactionAsync();

            await CreateProcessor().RevertGatePassAsync(id, 5, conn, tx);

            await tx.CommitAsync();

            (await GetGeFlagAsync(id)).Should().BeFalse();
        }

        [Fact]
        public void DocumentType_Matches_Invoice_TransactionType()
        {
            CreateProcessor().DocumentType.Should().Be(SalesManagement.Domain.Common.MiscEnumEntity.TransactionTypeInvoice);
        }
    }
}
