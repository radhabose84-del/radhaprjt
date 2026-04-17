using InventoryManagement.Application.Common.Interfaces.IMiscMaster;
using InventoryManagement.Domain.Common;
using InventoryManagement.Domain.Entities;
using InventoryManagement.Domain.Entities.Issue;
using InventoryManagement.Domain.Entities.MRS;
using InventoryManagement.Domain.Entities.Stock;
using InventoryManagement.Infrastructure.Data;
using InventoryManagement.Infrastructure.Repositories.Issue;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.IntegrationTests.Repositories.Issue
{
    [Collection("DatabaseCollection")]
    public sealed class IssueEntryCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public IssueEntryCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private IssueEntryCommandRepository CreateRepository(ApplicationDbContext ctx) =>
            new(ctx, _fixture.IpMock.Object);

        private async Task ClearTablesAsync(ApplicationDbContext ctx) => await _fixture.ClearAllTablesAsync();

        private async Task<int> SeedMrsHeaderAsync()
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            var typeMaster = new InventoryManagement.Domain.Entities.MiscTypeMaster
            {
                MiscTypeCode = "ApprovalStatus", Description = "Approval Status",
                IsActive = BaseEntity.Status.Active, IsDeleted = BaseEntity.IsDelete.NotDeleted
            };
            ctx.MiscTypeMaster.Add(typeMaster);
            await ctx.SaveChangesAsync();

            var status = new InventoryManagement.Domain.Entities.MiscMaster
            {
                MiscTypeId = typeMaster.Id, Code = "Pending", Description = "Pending",
                IsActive = BaseEntity.Status.Active, IsDeleted = BaseEntity.IsDelete.NotDeleted
            };
            ctx.MiscMaster.Add(status);
            await ctx.SaveChangesAsync();

            var catType = new InventoryManagement.Domain.Entities.MiscTypeMaster
            {
                MiscTypeCode = "RequestCategory", Description = "Request Category",
                IsActive = BaseEntity.Status.Active, IsDeleted = BaseEntity.IsDelete.NotDeleted
            };
            ctx.MiscTypeMaster.Add(catType);
            await ctx.SaveChangesAsync();

            var category = new InventoryManagement.Domain.Entities.MiscMaster
            {
                MiscTypeId = catType.Id, Code = "GeneralStore", Description = "General Store",
                IsActive = BaseEntity.Status.Active, IsDeleted = BaseEntity.IsDelete.NotDeleted
            };
            ctx.MiscMaster.Add(category);
            await ctx.SaveChangesAsync();

            var mrsHeader = new MrsHeader
            {
                MrsNo = "MRS-ISS-001",
                MrsDate = DateTimeOffset.UtcNow,
                UnitId = 1,
                RequestCategoryId = category.Id,
                DepartmentId = 1, SubDepartmentId = 1,
                StatusId = status.Id,
                CreatedBy = 1, CreatedByName = "test-user",
                CreatedIP = "127.0.0.1",
                CreatedDate = DateTimeOffset.UtcNow
            };
            ctx.MrsHeader.Add(mrsHeader);
            await ctx.SaveChangesAsync();

            return mrsHeader.Id;
        }

        [Fact]
        public async Task GenerateNextCodeAsync_Should_Return_NonEmpty_Code()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var code = await CreateRepository(ctx).GenerateNextCodeAsync(CancellationToken.None);

            code.Should().NotBeNullOrWhiteSpace();
            code.Should().StartWith("ISS-");
        }

        [Fact]
        public async Task CreateIssueWithLedgersAsync_Should_Return_PositiveId()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var mrsId = await SeedMrsHeaderAsync();

            var header = new IssueHeader
            {
                IssueNo = "ISS-1-0001",
                IssueDate = DateTimeOffset.UtcNow,
                UnitId = 1,
                MrsHeaderId = mrsId,
                IssuedBy = 1,
                IssuedDate = DateTimeOffset.UtcNow,
                IssuedByName = "test-user",
                IssuedIp = "127.0.0.1",
                IssueHeaderName = new List<IssueDetail>()
            };

            await using var ctx2 = _fixture.CreateFreshDbContext();
            var id = await new IssueEntryCommandRepository(ctx2, _fixture.IpMock.Object)
                .CreateIssueWithLedgersAsync(header, new List<StockLedger>(), new List<SubStoreStockLedger>(), null!);

            id.Should().BeGreaterThan(0);
        }
    }
}
