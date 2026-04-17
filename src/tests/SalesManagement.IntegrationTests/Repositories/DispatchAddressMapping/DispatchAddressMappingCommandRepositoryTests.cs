using Microsoft.EntityFrameworkCore;
using SalesManagement.Infrastructure.Data;
using SalesManagement.Infrastructure.Repositories.DispatchAddressMapping;
using SalesManagement.IntegrationTests.Common;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.IntegrationTests.Repositories.DispatchAddressMapping
{
    [Collection("DatabaseCollection")]
    public sealed class DispatchAddressMappingCommandRepositoryTests
    {
        private readonly DbFixture _fixture;
        public DispatchAddressMappingCommandRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private DispatchAddressMappingCommandRepository CreateRepo(ApplicationDbContext ctx) => new(ctx);

        private async Task<int> EnsureMiscIdAsync(string code = "DAM_USG")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var t = await ctx.MiscTypeMaster.FirstOrDefaultAsync(x => x.MiscTypeCode == "DAM_MT");
            if (t == null)
            {
                t = new SalesManagement.Domain.Entities.MiscTypeMaster
                {
                    MiscTypeCode = "DAM_MT", Description = "T",
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MiscTypeMaster.AddAsync(t);
                await ctx.SaveChangesAsync();
            }
            var m = await ctx.MiscMaster.FirstOrDefaultAsync(x => x.Code == code);
            if (m == null)
            {
                m = new SalesManagement.Domain.Entities.MiscMaster
                {
                    MiscTypeId = t.Id, Code = code, Description = code,
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MiscMaster.AddAsync(m);
                await ctx.SaveChangesAsync();
            }
            return m.Id;
        }

        private async Task<int> EnsureDispatchAddressAsync(string name = "DAMM_ADDR")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var existing = await ctx.DispatchAddressMaster.FirstOrDefaultAsync(x => x.DispatchAddressName == name);
            if (existing != null) return existing.Id;
            var d = new SalesManagement.Domain.Entities.DispatchAddressMaster
            {
                DispatchAddressName = name,
                AddressLine1 = "L1",
                CityId = 1, StateId = 1, CountryId = 1,
                PinCode = "560001",
                FreightId = 1,
                IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
            };
            await ctx.DispatchAddressMaster.AddAsync(d);
            await ctx.SaveChangesAsync();
            return d.Id;
        }

        private async Task<SalesManagement.Domain.Entities.DispatchAddressMapping> BuildEntityAsync(int partyId = 1)
        {
            var addrId = await EnsureDispatchAddressAsync();
            var usageId = await EnsureMiscIdAsync();
            return new SalesManagement.Domain.Entities.DispatchAddressMapping
            {
                PartyId = partyId,
                DispatchAddressId = addrId,
                UsageTypeId = usageId,
                IsDefault = false,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
        }

        private async Task ClearAsync(ApplicationDbContext ctx) =>
            await _fixture.ClearAllTablesAsync();

        [Fact]
        public async Task CreateAsync_Should_Return_NewId()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);

            var id = await CreateRepo(ctx).CreateAsync(await BuildEntityAsync(partyId: 1));

            id.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);

            var entity = await BuildEntityAsync(partyId: 5);
            entity.IsDefault = true;
            var id = await CreateRepo(ctx).CreateAsync(entity);
            ctx.ChangeTracker.Clear();
            var saved = await ctx.DispatchAddressMapping.FirstAsync(x => x.Id == id);

            saved.PartyId.Should().Be(5);
            saved.IsDefault.Should().BeTrue();
        }

        [Fact]
        public async Task UpdateAsync_Should_Persist_Mutable_Fields_Only()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);
            var id = await CreateRepo(ctx).CreateAsync(await BuildEntityAsync(partyId: 7));
            ctx.ChangeTracker.Clear();

            var entity = await BuildEntityAsync(partyId: 99); // PartyId is immutable
            entity.Id = id;
            entity.IsDefault = true;
            entity.IsActive = Status.Inactive;

            var result = await CreateRepo(ctx).UpdateAsync(entity);
            ctx.ChangeTracker.Clear();

            result.Should().Be(id);
            var reloaded = await ctx.DispatchAddressMapping.FirstAsync(x => x.Id == id);
            reloaded.PartyId.Should().Be(7); // not changed
            reloaded.IsDefault.Should().BeTrue();
            reloaded.IsActive.Should().Be(Status.Inactive);
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
        public async Task SoftDeleteAsync_Should_Return_True_When_Successful()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);
            var id = await CreateRepo(ctx).CreateAsync(await BuildEntityAsync());
            ctx.ChangeTracker.Clear();

            var result = await CreateRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Flag_Record()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);
            var id = await CreateRepo(ctx).CreateAsync(await BuildEntityAsync());
            ctx.ChangeTracker.Clear();

            await CreateRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var reloaded = await ctx.DispatchAddressMapping.FirstAsync(x => x.Id == id);
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
