using Microsoft.EntityFrameworkCore;
using SalesManagement.Infrastructure.Data;
using SalesManagement.Infrastructure.Repositories.CustomerVisit;
using SalesManagement.IntegrationTests.Common;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.IntegrationTests.Repositories.CustomerVisit
{
    [Collection("DatabaseCollection")]
    public sealed class CustomerVisitCommandRepositoryTests
    {
        private readonly DbFixture _fixture;
        public CustomerVisitCommandRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private CustomerVisitCommandRepository CreateRepo(ApplicationDbContext ctx) => new(ctx);

        private async Task<int> EnsureMarketingOfficerAsync()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var existing = await ctx.MarketingOfficer.FirstOrDefaultAsync(x => x.EmployeeNo == "CVC_MO");
            if (existing != null) return existing.Id;

            var org = await ctx.SalesOrganisation.FirstOrDefaultAsync(x => x.SalesOrganisationCode == "CVC_ORG");
            if (org == null)
            {
                org = new SalesManagement.Domain.Entities.SalesOrganisation
                {
                    SalesOrganisationCode = "CVC_ORG", SalesOrganisationName = "Org",
                    CompanyId = 1, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.SalesOrganisation.AddAsync(org);
                await ctx.SaveChangesAsync();
            }
            var office = await ctx.SalesOffice.FirstOrDefaultAsync(x => x.SalesOfficeName == "CVC_OFC");
            if (office == null)
            {
                office = new SalesManagement.Domain.Entities.SalesOffice
                {
                    SalesOfficeName = "CVC_OFC", SalesOrganisationId = org.Id,
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.SalesOffice.AddAsync(office);
                await ctx.SaveChangesAsync();
            }
            var mo = new SalesManagement.Domain.Entities.MarketingOfficer
            {
                EmployeeNo = "CVC_MO", EmployeeName = "Officer CVC",
                MobileNo = "9876543210", Email = "mo@y.com",
                Unit = "U", Department = "Sales", Designation = "Mgr",
                SalesOfficeId = office.Id,
                IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
            };
            await ctx.MarketingOfficer.AddAsync(mo);
            await ctx.SaveChangesAsync();
            return mo.Id;
        }

        private async Task<int> EnsureVisitTypeAsync()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var t = await ctx.MiscTypeMaster.FirstOrDefaultAsync(x => x.MiscTypeCode == "CVC_MT");
            if (t == null)
            {
                t = new SalesManagement.Domain.Entities.MiscTypeMaster
                {
                    MiscTypeCode = "CVC_MT", Description = "T",
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MiscTypeMaster.AddAsync(t);
                await ctx.SaveChangesAsync();
            }
            var m = await ctx.MiscMaster.FirstOrDefaultAsync(x => x.Code == "CVC_VT");
            if (m == null)
            {
                m = new SalesManagement.Domain.Entities.MiscMaster
                {
                    MiscTypeId = t.Id, Code = "CVC_VT", Description = "Visit",
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MiscMaster.AddAsync(m);
                await ctx.SaveChangesAsync();
            }
            return m.Id;
        }

        private async Task<SalesManagement.Domain.Entities.CustomerVisit> BuildEntityAsync(
            int customerId = 1, string remarks = "test", List<int>? itemIds = null)
        {
            var moId = await EnsureMarketingOfficerAsync();
            var vtId = await EnsureVisitTypeAsync();
            var entity = new SalesManagement.Domain.Entities.CustomerVisit
            {
                CustomerId = customerId,
                VisitTypeId = vtId,
                VisitDateTime = DateTimeOffset.UtcNow,
                Latitude = 12.97m, Longitude = 77.59m,
                ImageName = "photo.jpg",
                Remarks = remarks,
                MarketingOfficerId = moId,
                IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
            };
            if (itemIds != null)
            {
                entity.CustomerVisitProducts = itemIds.Select(i =>
                    new SalesManagement.Domain.Entities.CustomerVisitProduct { ItemId = i }).ToList();
            }
            return entity;
        }

        private async Task ClearAsync(ApplicationDbContext ctx) =>
            await _fixture.ClearAllTablesAsync();

        [Fact]
        public async Task CreateAsync_Should_Return_NewId()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);

            var id = await CreateRepo(ctx).CreateAsync(await BuildEntityAsync(1, "visit1"));

            id.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Products()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);

            var id = await CreateRepo(ctx).CreateAsync(await BuildEntityAsync(1, "with products", new List<int> { 10, 20 }));

            var products = await ctx.CustomerVisitProduct.Where(p => p.CustomerVisitId == id).ToListAsync();
            products.Should().HaveCount(2);
            products.Select(p => p.ItemId).Should().BeEquivalentTo(new[] { 10, 20 });
        }

        [Fact]
        public async Task UpdateAsync_Should_Persist_Header_Changes()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);
            var id = await CreateRepo(ctx).CreateAsync(await BuildEntityAsync(1, "orig"));
            ctx.ChangeTracker.Clear();

            var entity = await BuildEntityAsync(5, "updated");
            entity.Id = id;

            var result = await CreateRepo(ctx).UpdateAsync(entity);
            ctx.ChangeTracker.Clear();

            result.Should().Be(id);
            var reloaded = await ctx.CustomerVisit.FirstAsync(x => x.Id == id);
            reloaded.CustomerId.Should().Be(5);
            reloaded.Remarks.Should().Be("updated");
        }

        [Fact]
        public async Task UpdateAsync_Should_Replace_Products()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);
            var id = await CreateRepo(ctx).CreateAsync(await BuildEntityAsync(1, "init", new List<int> { 1, 2 }));
            ctx.ChangeTracker.Clear();

            var updated = await BuildEntityAsync(1, "init", new List<int> { 3, 4, 5 });
            updated.Id = id;

            await CreateRepo(ctx).UpdateAsync(updated);
            ctx.ChangeTracker.Clear();

            var products = await ctx.CustomerVisitProduct.Where(p => p.CustomerVisitId == id).ToListAsync();
            products.Select(p => p.ItemId).Should().BeEquivalentTo(new[] { 3, 4, 5 });
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_Zero_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            var ghost = await BuildEntityAsync(1, "ghost");
            ghost.Id = 9999999;

            var result = await CreateRepo(ctx).UpdateAsync(ghost);

            result.Should().Be(0);
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Flag_Record()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);
            var id = await CreateRepo(ctx).CreateAsync(await BuildEntityAsync(1, "del"));
            ctx.ChangeTracker.Clear();

            await CreateRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var reloaded = await ctx.CustomerVisit.FirstAsync(x => x.Id == id);
            reloaded.IsDeleted.Should().Be(IsDelete.Deleted);
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_False_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            var result = await CreateRepo(ctx).SoftDeleteAsync(9999999, CancellationToken.None);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task UpdateImageNameAsync_Should_Set_ImageName()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);
            var id = await CreateRepo(ctx).CreateAsync(await BuildEntityAsync(1, "imgtest"));
            ctx.ChangeTracker.Clear();

            var result = await CreateRepo(ctx).UpdateImageNameAsync(id, "new-photo.png", CancellationToken.None);
            ctx.ChangeTracker.Clear();

            result.Should().BeTrue();
            var reloaded = await ctx.CustomerVisit.FirstAsync(x => x.Id == id);
            reloaded.ImageName.Should().Be("new-photo.png");
        }

        [Fact]
        public async Task UpdateImageNameAsync_Should_Return_False_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            var result = await CreateRepo(ctx).UpdateImageNameAsync(9999999, "x.png", CancellationToken.None);

            result.Should().BeFalse();
        }
    }
}
