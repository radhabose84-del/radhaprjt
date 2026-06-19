using System.Data;
using Contracts.Interfaces.Lookups.Finance;
using Microsoft.EntityFrameworkCore;
using PurchaseManagement.Domain.Entities.BlanketMaster;
using PurchaseManagement.Infrastructure.Data;
using PurchaseManagement.Infrastructure.Repositories.BlanketMaster;
using PurchaseManagement.IntegrationTests.Common;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.IntegrationTests.Repositories.BlanketMaster
{
    [Collection("DatabaseCollection")]
    public sealed class BlanketMasterCommandRepositoryTests
    {
        private readonly DbFixture _fixture;
        public BlanketMasterCommandRepositoryTests(DbFixture fixture) => _fixture = fixture;

        // GenerateDocumentNumber stubbed → sets BlanketNumber; IncrementDocNoAsync no-op.
        private BlanketMasterCommandRepository CreateRepo(ApplicationDbContext ctx, string blanketNumber)
        {
            var docSeq = new Mock<IDocumentSequenceLookup>(MockBehavior.Loose);
            docSeq.Setup(d => d.GenerateDocumentNumber(It.IsAny<int>()))
                  .ReturnsAsync(new List<string> { blanketNumber });
            docSeq.Setup(d => d.IncrementDocNoAsync(It.IsAny<int>(), It.IsAny<IDbConnection>(), It.IsAny<IDbTransaction>()))
                  .Returns(Task.CompletedTask);
            return new BlanketMasterCommandRepository(ctx, _fixture.IpMock.Object, docSeq.Object);
        }

        // StatusId + ProcurementTypeId are same-module MiscMaster FKs → seed one MiscMaster row.
        private async Task<int> EnsureMiscAsync(ApplicationDbContext ctx)
        {
            var mt = new PurchaseManagement.Domain.Entities.MiscTypeMaster
            {
                MiscTypeCode = "BLK_MT", Description = "Blanket Type",
                IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
            };
            await ctx.Set<PurchaseManagement.Domain.Entities.MiscTypeMaster>().AddAsync(mt);
            await ctx.SaveChangesAsync();
            var m = new PurchaseManagement.Domain.Entities.MiscMaster
            {
                Code = "BLK_MSC", Description = "Blanket Misc", MiscTypeId = mt.Id,
                IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
            };
            await ctx.Set<PurchaseManagement.Domain.Entities.MiscMaster>().AddAsync(m);
            await ctx.SaveChangesAsync();
            return m.Id;
        }

        private static BlanketHeader BuildEntity(int miscId)
        {
            var now = new DateTimeOffset(2026, 1, 15, 0, 0, 0, TimeSpan.Zero);
            return new BlanketHeader
            {
                UnitId = 1,
                BlanketDate = now,
                VendorId = 100,
                CurrencyId = 1,
                ProcurementTypeId = miscId,
                BrokerName = "Broker A",
                ValidityFrom = now,
                ValidityTo = now.AddMonths(6),
                StatusId = miscId,
                TotalEstimatedValue = 1000m,
                Remarks = "blanket",
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted,
                Details = new List<BlanketDetail>
                {
                    new()
                    {
                        ItemSno = 1, ItemId = 10, UOMId = 1,
                        EstimatedQuantity = 100m, Rate = 10m, TotalPrice = 1000m,
                        HSNId = null, GSTPercentage = 18m, QualitySpecification = "spec",
                        IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted,
                        Schedules = new List<BlanketSchedule>
                        {
                            new()
                            {
                                ScheduleNo = 1, ScheduleDate = now.AddDays(30),
                                ScheduleQuantity = 50m, Remarks = "sch",
                                IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                            }
                        }
                    }
                }
            };
        }

        private async Task ClearAsync() => await _fixture.ClearAllTablesAsync();

        [Fact]
        public async Task CreateAsync_Should_Set_BlanketNumber_And_Return_Entity()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var miscId = await EnsureMiscAsync(ctx);

            var created = await CreateRepo(ctx, "BLK_C1").CreateAsync(BuildEntity(miscId), 1, CancellationToken.None);

            created.Id.Should().BeGreaterThan(0);
            created.BlanketNumber.Should().Be("BLK_C1");
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Header_Detail_Schedule()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var miscId = await EnsureMiscAsync(ctx);

            var created = await CreateRepo(ctx, "BLK_C2").CreateAsync(BuildEntity(miscId), 1, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.Set<BlanketHeader>().FirstAsync(x => x.Id == created.Id);
            saved.BlanketNumber.Should().Be("BLK_C2");
            saved.VendorId.Should().Be(100);

            var detailCount = await ctx.Set<BlanketDetail>().CountAsync(d => d.BlanketHeaderId == created.Id);
            detailCount.Should().Be(1);

            var detail = await ctx.Set<BlanketDetail>().FirstAsync(d => d.BlanketHeaderId == created.Id);
            var scheduleCount = await ctx.Set<BlanketSchedule>().CountAsync(s => s.BlanketDetailId == detail.Id);
            scheduleCount.Should().Be(1);
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_True_And_Flag()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var miscId = await EnsureMiscAsync(ctx);
            var created = await CreateRepo(ctx, "BLK_D1").CreateAsync(BuildEntity(miscId), 1, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var result = await CreateRepo(ctx, "X").SoftDeleteAsync(created.Id, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            result.Should().BeTrue();
            var reloaded = await ctx.Set<BlanketHeader>().IgnoreQueryFilters().FirstAsync(x => x.Id == created.Id);
            reloaded.IsDeleted.Should().Be(IsDelete.Deleted);
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_False_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var result = await CreateRepo(ctx, "X").SoftDeleteAsync(9999999, CancellationToken.None);
            result.Should().BeFalse();
        }
    }
}
