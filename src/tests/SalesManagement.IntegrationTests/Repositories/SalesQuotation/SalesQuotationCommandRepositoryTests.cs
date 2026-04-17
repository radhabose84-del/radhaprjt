using Microsoft.EntityFrameworkCore;
using SalesManagement.Infrastructure.Data;
using SalesManagement.Infrastructure.Repositories.SalesQuotation;
using SalesManagement.IntegrationTests.Common;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.IntegrationTests.Repositories.SalesQuotation
{
    [Collection("DatabaseCollection")]
    public sealed class SalesQuotationCommandRepositoryTests
    {
        private readonly DbFixture _fixture;
        public SalesQuotationCommandRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private SalesQuotationCommandRepository CreateRepo(ApplicationDbContext ctx) => new(ctx);

        private async Task<int> EnsureMiscAsync(ApplicationDbContext ctx, int miscTypeId, string code)
        {
            var m = await ctx.MiscMaster.FirstOrDefaultAsync(x => x.MiscTypeId == miscTypeId && x.Code == code);
            if (m == null)
            {
                m = new SalesManagement.Domain.Entities.MiscMaster
                {
                    MiscTypeId = miscTypeId, Code = code, Description = code,
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MiscMaster.AddAsync(m);
                await ctx.SaveChangesAsync();
            }
            return m.Id;
        }

        private async Task<int> EnsureDeliveryTermAsync()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var mt = await ctx.MiscTypeMaster.FirstOrDefaultAsync(x => x.MiscTypeCode == "SQC_MT");
            if (mt == null)
            {
                mt = new SalesManagement.Domain.Entities.MiscTypeMaster
                {
                    MiscTypeCode = "SQC_MT", Description = "Quotation misc",
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MiscTypeMaster.AddAsync(mt);
                await ctx.SaveChangesAsync();
            }
            return await EnsureMiscAsync(ctx, mt.Id, "SQC_DT");
        }

        private async Task<SalesManagement.Domain.Entities.SalesQuotationHeader> BuildEntityAsync(int customerId = 100, int detailCount = 2)
        {
            var dtId = await EnsureDeliveryTermAsync();
            var entity = new SalesManagement.Domain.Entities.SalesQuotationHeader
            {
                CustomerId = customerId,
                QuotationDate = DateOnly.FromDateTime(DateTime.UtcNow.Date),
                ValidityDate = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(30)),
                PaymentTermId = 1,  // cross-module, no DB constraint
                DeliveryTermId = dtId,
                FreightCharges = 100m, OtherCharges = 50m,
                TotalBasicAmount = 1000m, TotalDiscount = 100m,
                NetTaxableAmount = 900m, TotalTax = 45m, GrandTotal = 945m,
                Remarks = "test quotation",
                IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted,
                SalesQuotationDetails = Enumerable.Range(1, detailCount).Select(i =>
                    new SalesManagement.Domain.Entities.SalesQuotationDetail
                    {
                        ItemId = i * 10, Quantity = i * 5m,
                        ExMillRate = 100m, Discount = 5m,
                        NetRate = 95m, TotalAmount = 475m,
                        HSNId = 1, TaxPercentage = 5m, TaxAmount = 23.75m
                    }).ToList()
            };
            return entity;
        }

        private async Task ClearAsync(ApplicationDbContext ctx) =>
            await _fixture.ClearAllTablesAsync();

        [Fact]
        public async Task CreateAsync_Should_Return_NewId()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);

            var id = await CreateRepo(ctx).CreateAsync(await BuildEntityAsync(customerId: 100));

            id.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Header_And_Details()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);

            var id = await CreateRepo(ctx).CreateAsync(await BuildEntityAsync(customerId: 200, detailCount: 3));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.SalesQuotationHeader.FirstAsync(x => x.Id == id);
            saved.CustomerId.Should().Be(200);

            var details = await ctx.SalesQuotationDetail.Where(d => d.SalesQuotationHeaderId == id).ToListAsync();
            details.Should().HaveCount(3);
        }

        [Fact]
        public async Task UpdateAsync_Should_Persist_Header_Changes()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);
            var id = await CreateRepo(ctx).CreateAsync(await BuildEntityAsync(customerId: 100));
            ctx.ChangeTracker.Clear();

            var updated = await BuildEntityAsync(customerId: 999);
            updated.Id = id;
            updated.Remarks = "updated quotation";
            updated.IsActive = Status.Inactive;

            var result = await CreateRepo(ctx).UpdateAsync(updated);
            ctx.ChangeTracker.Clear();

            result.Should().Be(id);
            var reloaded = await ctx.SalesQuotationHeader.FirstAsync(x => x.Id == id);
            reloaded.CustomerId.Should().Be(999);
            reloaded.Remarks.Should().Be("updated quotation");
            reloaded.IsActive.Should().Be(Status.Inactive);
        }

        [Fact]
        public async Task UpdateAsync_Should_Replace_Details()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);
            var id = await CreateRepo(ctx).CreateAsync(await BuildEntityAsync(detailCount: 2));
            ctx.ChangeTracker.Clear();

            var updated = await BuildEntityAsync(detailCount: 4);
            updated.Id = id;

            await CreateRepo(ctx).UpdateAsync(updated);
            ctx.ChangeTracker.Clear();

            var details = await ctx.SalesQuotationDetail.Where(d => d.SalesQuotationHeaderId == id).ToListAsync();
            details.Should().HaveCount(4);
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_Zero_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            var ghost = await BuildEntityAsync();
            ghost.Id = 9999999;

            var result = await CreateRepo(ctx).UpdateAsync(ghost);

            result.Should().Be(0);
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Flag_Record()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);
            var id = await CreateRepo(ctx).CreateAsync(await BuildEntityAsync());
            ctx.ChangeTracker.Clear();

            var result = await CreateRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            result.Should().BeTrue();
            var reloaded = await ctx.SalesQuotationHeader.IgnoreQueryFilters().FirstAsync(x => x.Id == id);
            reloaded.IsDeleted.Should().Be(IsDelete.Deleted);
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_False_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            var result = await CreateRepo(ctx).SoftDeleteAsync(9999999, CancellationToken.None);

            result.Should().BeFalse();
        }
    }
}
