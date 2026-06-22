using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PurchaseManagement.Domain.Entities.BlanketMaster;
using PurchaseManagement.Infrastructure.Repositories.BlanketMaster;
using PurchaseManagement.IntegrationTests.Common;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.IntegrationTests.Repositories.BlanketMaster
{
    [Collection("DatabaseCollection")]
    public sealed class BlanketMasterQueryRepositoryTests
    {
        private readonly DbFixture _fixture;
        public BlanketMasterQueryRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private BlanketMasterQueryRepository CreateRepo() =>
            new(new SqlConnection(_fixture.ConnectionString));

        private async Task<int> EnsureMiscAsync()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var mt = new PurchaseManagement.Domain.Entities.MiscTypeMaster
            {
                MiscTypeCode = "BLKQ_MT", Description = "Blanket Type",
                IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
            };
            await ctx.Set<PurchaseManagement.Domain.Entities.MiscTypeMaster>().AddAsync(mt);
            await ctx.SaveChangesAsync();
            var m = new PurchaseManagement.Domain.Entities.MiscMaster
            {
                Code = "BLKQ_MSC", Description = "Blanket Misc", MiscTypeId = mt.Id,
                IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
            };
            await ctx.Set<PurchaseManagement.Domain.Entities.MiscMaster>().AddAsync(m);
            await ctx.SaveChangesAsync();
            return m.Id;
        }

        private async Task<int> SeedAsync(int miscId, string blanketNumber, bool withDetail = false,
            DateTimeOffset? validityTo = null, IsDelete deleted = IsDelete.NotDeleted)
        {
            var now = new DateTimeOffset(2026, 1, 15, 0, 0, 0, TimeSpan.Zero);
            await using var ctx = _fixture.CreateFreshDbContext();
            var h = new BlanketHeader
            {
                UnitId = 1,
                BlanketNumber = blanketNumber,
                BlanketDate = now,
                VendorId = 100,
                CurrencyId = 1,
                ProcurementTypeId = miscId,
                ValidityFrom = now,
                ValidityTo = validityTo ?? now.AddMonths(6),
                StatusId = miscId,
                TotalEstimatedValue = 1000m,
                Remarks = "blanket",
                IsActive = Status.Active,
                IsDeleted = deleted
            };
            if (withDetail)
            {
                h.Details = new List<BlanketDetail>
                {
                    new()
                    {
                        ItemSno = 1, ItemId = 10, UOMId = 1,
                        EstimatedQuantity = 100m, Rate = 10m, TotalPrice = 1000m,
                        GSTPercentage = 18m,
                        IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted,
                        Schedules = new List<BlanketSchedule>
                        {
                            new()
                            {
                                ScheduleNo = 1, ScheduleDate = now.AddDays(30), ScheduleQuantity = 50m,
                                IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                            }
                        }
                    }
                };
            }
            await ctx.Set<BlanketHeader>().AddAsync(h);
            await ctx.SaveChangesAsync();
            return h.Id;
        }

        private async Task ClearAsync() => await _fixture.ClearAllTablesAsync();

        [Fact]
        public async Task GetAllAsync_Should_Return_Seeded()
        {
            await ClearAsync();
            var miscId = await EnsureMiscAsync();
            await SeedAsync(miscId, "BLKQ1");

            var (rows, total) = await CreateRepo().GetAllAsync(1, 10, null, CancellationToken.None);

            rows.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllAsync_Should_Exclude_SoftDeleted()
        {
            await ClearAsync();
            var miscId = await EnsureMiscAsync();
            await SeedAsync(miscId, "BLKQDEL", deleted: IsDelete.Deleted);

            var (_, total) = await CreateRepo().GetAllAsync(1, 10, null, CancellationToken.None);

            total.Should().Be(0);
        }

        [Fact]
        public async Task GetAllAsync_Should_Filter_By_SearchTerm()
        {
            await ClearAsync();
            var miscId = await EnsureMiscAsync();
            await SeedAsync(miscId, "BLKQ_UNIQ");
            await SeedAsync(miscId, "BLKQ_OTHER");

            var (rows, _) = await CreateRepo().GetAllAsync(1, 10, "BLKQ_UNIQ", CancellationToken.None);

            rows.Should().HaveCount(1);
            rows[0].BlanketNumber.Should().Be("BLKQ_UNIQ");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Header_With_Details()
        {
            await ClearAsync();
            var miscId = await EnsureMiscAsync();
            var id = await SeedAsync(miscId, "BLKQ_GBI", withDetail: true);

            var result = await CreateRepo().GetByIdAsync(id, CancellationToken.None);

            result.Should().NotBeNull();
            result!.BlanketNumber.Should().Be("BLKQ_GBI");
            result.Details.Should().HaveCount(1);
            result.Details[0].Schedules.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_SoftDeleted()
        {
            await ClearAsync();
            var miscId = await EnsureMiscAsync();
            var id = await SeedAsync(miscId, "BLKQ_GSD", deleted: IsDelete.Deleted);

            var result = await CreateRepo().GetByIdAsync(id, CancellationToken.None);

            result.Should().BeNull();
        }

        [Fact]
        public async Task NotFoundAsync_Should_Return_True_For_Missing()
        {
            var result = await CreateRepo().NotFoundAsync(9999999, CancellationToken.None);
            result.Should().BeTrue();
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_True_For_Duplicate()
        {
            await ClearAsync();
            var miscId = await EnsureMiscAsync();
            await SeedAsync(miscId, "BLKQ_DUP");

            var result = await CreateRepo().AlreadyExistsAsync("BLKQ_DUP");

            result.Should().BeTrue();
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Exclude_Self()
        {
            await ClearAsync();
            var miscId = await EnsureMiscAsync();
            var id = await SeedAsync(miscId, "BLKQ_SELF");

            var result = await CreateRepo().AlreadyExistsAsync("BLKQ_SELF", id);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task IsExpiredAsync_Should_Return_True_For_Past_Validity()
        {
            await ClearAsync();
            var miscId = await EnsureMiscAsync();
            var id = await SeedAsync(miscId, "BLKQ_EXP", validityTo: new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero));

            var result = await CreateRepo().IsExpiredAsync(id, CancellationToken.None);

            result.Should().BeTrue();
        }
    }
}
