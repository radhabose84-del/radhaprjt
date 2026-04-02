using Contracts.Interfaces;
using InventoryManagement.Application.Common.Interfaces;
using InventoryManagement.Application.Common.Interfaces.IMiscMaster;
using InventoryManagement.Domain.Common;
using InventoryManagement.Domain.Entities;
using InventoryManagement.Domain.Entities.MRS;
using InventoryManagement.Infrastructure.Data;
using InventoryManagement.Infrastructure.Repositories.MRS;
using InventoryManagement.Infrastructure.Repositories.MiscMaster;
using InventoryManagement.Infrastructure.Repositories.MiscTypeMaster;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.IntegrationTests.Repositories.MRS
{
    [Collection("DatabaseCollection")]
    public sealed class MrsEntryCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public MrsEntryCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private MrsEntryCommandRepository CreateRepository(ApplicationDbContext ctx)
        {
            var miscRepo = new Mock<IMiscMasterQueryRepository>(MockBehavior.Loose);
            miscRepo.Setup(r => r.GetMiscMaster(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>()))
                .ReturnsAsync(new List<InventoryManagement.Domain.Entities.MiscMaster>());

            return new MrsEntryCommandRepository(ctx, _fixture.IpMock.Object, miscRepo.Object);
        }

        private async Task ClearTablesAsync(ApplicationDbContext ctx)
        {
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Inventory].[MrsDetail]");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Inventory].[MrsHeader]");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Inventory].[UOMConversion]");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Inventory].[UOM]");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Inventory].[MiscMaster]");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Inventory].[MiscTypeMaster]");
        }

        private async Task<(int statusId, int categoryId)> SeedMiscDataAsync()
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            // Seed MiscTypeMaster for ApprovalStatus
            var typeMaster = new InventoryManagement.Domain.Entities.MiscTypeMaster
            {
                MiscTypeCode = "ApprovalStatus",
                Description = "Approval Status",
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            };
            ctx.MiscTypeMaster.Add(typeMaster);
            await ctx.SaveChangesAsync();

            // Seed Pending status
            var pending = new InventoryManagement.Domain.Entities.MiscMaster
            {
                MiscTypeId = typeMaster.Id,
                Code = "Pending",
                Description = "Pending",
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            };
            ctx.MiscMaster.Add(pending);

            // Seed RequestCategory type
            var catType = new InventoryManagement.Domain.Entities.MiscTypeMaster
            {
                MiscTypeCode = "RequestCategory",
                Description = "Request Category",
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            };
            ctx.MiscTypeMaster.Add(catType);
            await ctx.SaveChangesAsync();

            var category = new InventoryManagement.Domain.Entities.MiscMaster
            {
                MiscTypeId = catType.Id,
                Code = "GeneralStore",
                Description = "General Store",
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            };
            ctx.MiscMaster.Add(category);
            await ctx.SaveChangesAsync();

            return (pending.Id, category.Id);
        }

        [Fact]
        public async Task GenerateNextCodeAsync_Should_Return_NonEmpty_Code()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var code = await CreateRepository(ctx).GenerateNextCodeAsync(CancellationToken.None);

            code.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_MrsHeader()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var (statusId, categoryId) = await SeedMiscDataAsync();

            var header = new MrsHeader
            {
                MrsNo = "MRS-TEST-001",
                MrsDate = DateTimeOffset.UtcNow,
                UnitId = 1,
                RequestCategoryId = categoryId,
                DepartmentId = 1,
                SubDepartmentId = 1,
                StatusId = statusId,
                CreatedBy = 1,
                CreatedByName = "test-user",
                CreatedIP = "127.0.0.1",
                CreatedDate = DateTimeOffset.UtcNow
            };

            await using var ctx2 = _fixture.CreateFreshDbContext();
            var result = await CreateRepository(ctx2).CreateAsync(header);

            result.Should().NotBeNull();
            result.Id.Should().BeGreaterThan(0);

            await using var ctx3 = _fixture.CreateFreshDbContext();
            var saved = await ctx3.MrsHeader.FirstOrDefaultAsync(x => x.Id == result.Id);
            saved.Should().NotBeNull();
            saved!.MrsNo.Should().Be("MRS-TEST-001");
        }
    }
}
