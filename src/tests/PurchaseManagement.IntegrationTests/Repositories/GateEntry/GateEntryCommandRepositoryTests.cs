using Contracts.Interfaces;
using Microsoft.EntityFrameworkCore;
using PurchaseManagement.Domain.Entities.GRN.GateEntry;
using PurchaseManagement.Infrastructure.Data;
using PurchaseManagement.Infrastructure.Repositories.GRN.GateEntry;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.IntegrationTests.Repositories.GateEntry
{
    [Collection("DatabaseCollection")]
    public sealed class GateEntryCommandRepositoryTests
    {
        private readonly DbFixture _fixture;
        public GateEntryCommandRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private GateEntryCommandRepository CreateRepo(ApplicationDbContext ctx)
        {
            var ip = new Mock<IIPAddressService>(MockBehavior.Loose);
            ip.Setup(x => x.GetUserId()).Returns(1);
            ip.Setup(x => x.GetUserName()).Returns("test-user");
            ip.Setup(x => x.GetUserIPAddress()).Returns("127.0.0.1");
            ip.Setup(x => x.GetUnitId()).Returns(1);
            return new GateEntryCommandRepository(ctx, ip.Object);
        }

        private async Task<int> EnsureReceivingTypeAsync()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var mt = await ctx.MiscTypeMaster.FirstOrDefaultAsync(x => x.MiscTypeCode == "GE_MT");
            if (mt == null)
            {
                mt = new PurchaseManagement.Domain.Entities.MiscTypeMaster
                {
                    MiscTypeCode = "GE_MT", Description = "Gate Entry",
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MiscTypeMaster.AddAsync(mt);
                await ctx.SaveChangesAsync();
            }
            var m = await ctx.MiscMaster.FirstOrDefaultAsync(x => x.Code == "GE_RT");
            if (m == null)
            {
                m = new PurchaseManagement.Domain.Entities.MiscMaster
                {
                    MiscTypeId = mt.Id, Code = "GE_RT", Description = "Receiving",
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MiscMaster.AddAsync(m);
                await ctx.SaveChangesAsync();
            }
            return m.Id;
        }

        private async Task<GateEntryHeader> BuildEntityAsync(string gateEntryNo = "GE001")
        {
            var rtId = await EnsureReceivingTypeAsync();
            return new GateEntryHeader
            {
                GateEntryNo = gateEntryNo,
                GateEntryDate = DateTimeOffset.UtcNow,
                UnitId = 1,
                PartyId = 100,
                VehicleNumber = "TN01AB1234",
                DriverName = "Driver",
                GrossWeight = 1000m,
                TareWeight = 200m,
                NetWeight = 800m,
                Remarks = "test",
                ReceivingTypeId = rtId,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
        }

        private async Task ClearAsync(ApplicationDbContext ctx)
        {
            await ctx.Database.ExecuteSqlRawAsync(@"
                DELETE FROM [Purchase].[GateEntryDetail];
                DELETE FROM [Purchase].[GateEntryHeader];");
        }

        [Fact]
        public async Task CreateAsync_Should_Return_NewId()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);

            var entity = await BuildEntityAsync("GE_C1");
            var id = await CreateRepo(ctx).CreateAsync(entity);

            id.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);

            var entity = await BuildEntityAsync("GE_C2");
            await CreateRepo(ctx).CreateAsync(entity);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.GateEntryHeader.FirstAsync(x => x.GateEntryNo == "GE_C2");
            saved.PartyId.Should().Be(100);
            saved.VehicleNumber.Should().Be("TN01AB1234");
            saved.NetWeight.Should().Be(800m);
        }

        [Fact]
        public async Task GenerateNextCodeAsync_Should_Return_Formatted_Code()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);

            var code = await CreateRepo(ctx).GenerateNextCodeAsync();

            code.Should().StartWith("GE-1-"); // UnitId=1
            code.Should().EndWith("01"); // first entry
        }

        [Fact]
        public async Task GenerateNextCodeAsync_Should_Increment_After_Create()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);
            var entity = await BuildEntityAsync();
            entity.GateEntryNo = "GE-1-01";
            await CreateRepo(ctx).CreateAsync(entity);

            var nextCode = await CreateRepo(ctx).GenerateNextCodeAsync();

            nextCode.Should().Be("GE-1-02");
        }
    }
}
