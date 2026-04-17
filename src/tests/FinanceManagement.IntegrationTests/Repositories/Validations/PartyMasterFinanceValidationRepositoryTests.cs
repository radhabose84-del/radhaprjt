using FinanceManagement.Domain.Common;
using FinanceManagement.Domain.Entities;
using FinanceManagement.Infrastructure.Repositories.Validations;
using FinanceManagement.IntegrationTests.Common;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FinanceManagement.IntegrationTests.Repositories.Validations
{
    [Collection("DatabaseCollection")]
    public sealed class PartyMasterFinanceValidationRepositoryTests
    {
        private readonly DbFixture _fixture;

        public PartyMasterFinanceValidationRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private PartyMasterFinanceValidationRepository CreateRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new PartyMasterFinanceValidationRepository(conn);
        }

        private async Task SeedEInvoiceAsync(
            int partyId,
            BaseEntity.Status active = BaseEntity.Status.Active,
            BaseEntity.IsDelete deleted = BaseEntity.IsDelete.NotDeleted,
            string? invoiceNo = null)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var entity = new FinanceManagement.Domain.Entities.EInvoiceHeader
            {
                UnitId = 1,
                DocType = "INV", SupplyType = "B2B",
                InvoiceNo = invoiceNo ?? $"P{partyId}-{Guid.NewGuid():N}"[..16],
                InvoiceDate = DateOnly.FromDateTime(DateTime.UtcNow),
                PlaceOfSupply = "33",
                IrnStatus = "Generated",
                PartyId = partyId,
                IsActive = active, IsDeleted = deleted
            };
            await ctx.EInvoiceHeader.AddAsync(entity);
            await ctx.SaveChangesAsync();
        }

        private async Task ClearAsync()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var wb = await ctx.EWaybillHeader.ToListAsync();
            ctx.EWaybillHeader.RemoveRange(wb);
            await ctx.SaveChangesAsync();
            var inv = await ctx.EInvoiceHeader.ToListAsync();
            ctx.EInvoiceHeader.RemoveRange(inv);
            await ctx.SaveChangesAsync();
        }

        // --- HasLinkedPartyMasterAsync ---

        [Fact]
        public async Task HasLinkedPartyMasterAsync_Should_Return_True_When_EInvoice_References_PartyId()
        {
            await ClearAsync();
            await SeedEInvoiceAsync(partyId: 101);

            var result = await CreateRepo().HasLinkedPartyMasterAsync(101);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task HasLinkedPartyMasterAsync_Should_Return_False_When_PartyId_Unused()
        {
            await ClearAsync();

            var result = await CreateRepo().HasLinkedPartyMasterAsync(99999);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task HasLinkedPartyMasterAsync_Should_Return_False_When_Invoice_SoftDeleted()
        {
            await ClearAsync();
            await SeedEInvoiceAsync(partyId: 202, deleted: BaseEntity.IsDelete.Deleted);

            var result = await CreateRepo().HasLinkedPartyMasterAsync(202);

            result.Should().BeFalse();
        }

        // --- HasActivePartyMasterAsync ---

        [Fact]
        public async Task HasActivePartyMasterAsync_Should_Return_True_When_Invoice_IsActive()
        {
            await ClearAsync();
            await SeedEInvoiceAsync(partyId: 303, active: BaseEntity.Status.Active);

            var result = await CreateRepo().HasActivePartyMasterAsync(303);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task HasActivePartyMasterAsync_Should_Return_False_When_Invoice_Inactive()
        {
            await ClearAsync();
            await SeedEInvoiceAsync(partyId: 404, active: BaseEntity.Status.Inactive);

            var result = await CreateRepo().HasActivePartyMasterAsync(404);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task HasActivePartyMasterAsync_Should_Return_False_When_Invoice_SoftDeleted()
        {
            await ClearAsync();
            await SeedEInvoiceAsync(partyId: 505, deleted: BaseEntity.IsDelete.Deleted);

            var result = await CreateRepo().HasActivePartyMasterAsync(505);

            result.Should().BeFalse();
        }
    }
}
