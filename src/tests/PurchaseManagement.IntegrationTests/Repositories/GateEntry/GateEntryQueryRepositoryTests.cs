using Contracts.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PurchaseManagement.Domain.Entities.GRN.GateEntry;
using PurchaseManagement.Infrastructure.Data;
using PurchaseManagement.Infrastructure.Repositories.GRN.GateEntry;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.IntegrationTests.Repositories.GateEntry
{
    [Collection("DatabaseCollection")]
    public sealed class GateEntryQueryRepositoryTests
    {
        private readonly DbFixture _fixture;
        public GateEntryQueryRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private GateEntryQueryRepository CreateRepo()
        {
            var ip = new Mock<IIPAddressService>(MockBehavior.Loose);
            ip.Setup(x => x.GetUnitId()).Returns(1);
            return new GateEntryQueryRepository(new SqlConnection(_fixture.ConnectionString), ip.Object);
        }

        private async Task<int> EnsureMiscTypeAsync(string code, string desc)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var mt = await ctx.MiscTypeMaster.FirstOrDefaultAsync(x => x.MiscTypeCode == code);
            if (mt == null)
            {
                mt = new PurchaseManagement.Domain.Entities.MiscTypeMaster
                {
                    MiscTypeCode = code, Description = desc,
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MiscTypeMaster.AddAsync(mt);
                await ctx.SaveChangesAsync();
            }
            return mt.Id;
        }

        [Fact]
        public async Task GetDocumentDirectoryAsync_Should_Return_Description_When_Exists()
        {
            // Seed MiscTypeMaster with GateEntryImage code
            await using var ctx = _fixture.CreateFreshDbContext();
            var mt = await ctx.MiscTypeMaster.FirstOrDefaultAsync(x => x.MiscTypeCode == "GateEntryImage");
            if (mt == null)
            {
                mt = new PurchaseManagement.Domain.Entities.MiscTypeMaster
                {
                    MiscTypeCode = "GateEntryImage", Description = "/uploads/gate-entry/",
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MiscTypeMaster.AddAsync(mt);
                await ctx.SaveChangesAsync();
            }

            var result = await CreateRepo().GetDocumentDirectoryAsync();

            result.Should().Be("/uploads/gate-entry/");
        }

        [Fact]
        public async Task GetDocumentDirectoryAsync_Should_Return_Null_When_Not_Found()
        {
            // Delete the specific MiscTypeMaster if it exists
            await using var ctx = _fixture.CreateFreshDbContext();
            var mt = await ctx.MiscTypeMaster.FirstOrDefaultAsync(x => x.MiscTypeCode == "GateEntryImage");
            if (mt != null)
            {
                mt.IsDeleted = IsDelete.Deleted;
                await ctx.SaveChangesAsync();
            }

            var result = await CreateRepo().GetDocumentDirectoryAsync();

            result.Should().BeNull();

            // Restore for other tests
            if (mt != null)
            {
                mt.IsDeleted = IsDelete.NotDeleted;
                await ctx.SaveChangesAsync();
            }
        }

        // Note: GetGateEntriesApprovedPoDto requires full PO chain
        // (PurchaseOrderHeader → PurchaseLocalHeader → PurchaseLocalDetail +
        //  GrnHeader/GrnDetail + MiscMaster with Approved status).
        // This heavy cross-entity setup is deferred — covered by unit tests.
    }
}
