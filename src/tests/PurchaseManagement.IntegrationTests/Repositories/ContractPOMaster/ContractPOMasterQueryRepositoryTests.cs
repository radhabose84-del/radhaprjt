using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PurchaseManagement.Domain.Entities.ContractPOMaster;
using PurchaseManagement.Infrastructure.Repositories.ContractPOMaster;
using PurchaseManagement.IntegrationTests.Common;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.IntegrationTests.Repositories.ContractPOMaster
{
    [Collection("DatabaseCollection")]
    public sealed class ContractPOMasterQueryRepositoryTests
    {
        private readonly DbFixture _fixture;
        public ContractPOMasterQueryRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private ContractPOMasterQueryRepository CreateRepo() =>
            new(new SqlConnection(_fixture.ConnectionString));

        private async Task<int> EnsureMiscAsync()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var mt = new PurchaseManagement.Domain.Entities.MiscTypeMaster
            {
                MiscTypeCode = "CPOQ_MT", Description = "Contract Type",
                IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
            };
            await ctx.Set<PurchaseManagement.Domain.Entities.MiscTypeMaster>().AddAsync(mt);
            await ctx.SaveChangesAsync();
            var m = new PurchaseManagement.Domain.Entities.MiscMaster
            {
                Code = "CPOQ_MSC", Description = "Contract Misc", MiscTypeId = mt.Id,
                IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
            };
            await ctx.Set<PurchaseManagement.Domain.Entities.MiscMaster>().AddAsync(m);
            await ctx.SaveChangesAsync();
            return m.Id;
        }

        private async Task<int> SeedAsync(int miscId, string contractNo, bool withDetail = false,
            IsDelete deleted = IsDelete.NotDeleted)
        {
            var now = new DateTimeOffset(2026, 1, 15, 0, 0, 0, TimeSpan.Zero);
            await using var ctx = _fixture.CreateFreshDbContext();
            var h = new ContractPOHeader
            {
                UnitId = 1,
                ContractPONumber = contractNo,
                ContractDate = now,
                VendorId = 100,
                CurrencyId = 1,
                ValidityFrom = now,
                ValidityTo = now.AddMonths(6),
                TotalContractValue = 1000m,
                UtilizedValue = 0m,
                BalanceValue = 1000m,
                StatusId = miscId,
                Remarks = "contract",
                IsActive = Status.Active,
                IsDeleted = deleted
            };
            if (withDetail)
            {
                h.ContractPODetails = new List<ContractPODetail>
                {
                    new()
                    {
                        ItemSno = 1, ItemId = 10, UOMId = 1,
                        ContractQuantity = 100m, ContractRate = 10m, ContractValue = 1000m,
                        UtilizedQuantity = 0m, BalanceQuantity = 100m, UtilizedValue = 0m, BalanceValue = 1000m,
                        GSTPercentage = 18m,
                        IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                    }
                };
            }
            await ctx.Set<ContractPOHeader>().AddAsync(h);
            await ctx.SaveChangesAsync();
            return h.Id;
        }

        private async Task ClearAsync() => await _fixture.ClearAllTablesAsync();

        [Fact]
        public async Task GetAllAsync_Should_Return_Seeded()
        {
            await ClearAsync();
            var miscId = await EnsureMiscAsync();
            await SeedAsync(miscId, "CPOQ1");

            var (rows, total) = await CreateRepo().GetAllAsync(1, 10, null, CancellationToken.None);

            rows.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllAsync_Should_Exclude_SoftDeleted()
        {
            await ClearAsync();
            var miscId = await EnsureMiscAsync();
            await SeedAsync(miscId, "CPOQDEL", deleted: IsDelete.Deleted);

            var (_, total) = await CreateRepo().GetAllAsync(1, 10, null, CancellationToken.None);

            total.Should().Be(0);
        }

        [Fact]
        public async Task GetAllAsync_Should_Filter_By_SearchTerm()
        {
            await ClearAsync();
            var miscId = await EnsureMiscAsync();
            await SeedAsync(miscId, "CPOQ_UNIQ");
            await SeedAsync(miscId, "CPOQ_OTHER");

            var (rows, _) = await CreateRepo().GetAllAsync(1, 10, "CPOQ_UNIQ", CancellationToken.None);

            rows.Should().HaveCount(1);
            rows[0].ContractPONumber.Should().Be("CPOQ_UNIQ");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Header_With_Details()
        {
            await ClearAsync();
            var miscId = await EnsureMiscAsync();
            var id = await SeedAsync(miscId, "CPOQ_GBI", withDetail: true);

            var result = await CreateRepo().GetByIdAsync(id, CancellationToken.None);

            result.Should().NotBeNull();
            result!.ContractPONumber.Should().Be("CPOQ_GBI");
            result.Details.Should().NotBeNull();
            result.Details!.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_SoftDeleted()
        {
            await ClearAsync();
            var miscId = await EnsureMiscAsync();
            var id = await SeedAsync(miscId, "CPOQ_GSD", deleted: IsDelete.Deleted);

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
            await SeedAsync(miscId, "CPOQ_DUP");

            var result = await CreateRepo().AlreadyExistsAsync("CPOQ_DUP");

            result.Should().BeTrue();
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Exclude_Self()
        {
            await ClearAsync();
            var miscId = await EnsureMiscAsync();
            var id = await SeedAsync(miscId, "CPOQ_SELF");

            var result = await CreateRepo().AlreadyExistsAsync("CPOQ_SELF", id);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task HasReleaseHistoryAsync_Should_Return_False_When_None()
        {
            await ClearAsync();
            var miscId = await EnsureMiscAsync();
            var id = await SeedAsync(miscId, "CPOQ_RH");

            var result = await CreateRepo().HasReleaseHistoryAsync(id);

            result.Should().BeFalse();
        }
    }
}
