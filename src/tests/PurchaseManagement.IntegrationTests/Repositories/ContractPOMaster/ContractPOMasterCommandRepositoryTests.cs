using System.Data;
using Contracts.Interfaces.Lookups.Finance;
using Microsoft.EntityFrameworkCore;
using PurchaseManagement.Domain.Entities.ContractPOMaster;
using PurchaseManagement.Infrastructure.Data;
using PurchaseManagement.Infrastructure.Repositories.ContractPOMaster;
using PurchaseManagement.IntegrationTests.Common;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.IntegrationTests.Repositories.ContractPOMaster
{
    [Collection("DatabaseCollection")]
    public sealed class ContractPOMasterCommandRepositoryTests
    {
        private readonly DbFixture _fixture;
        public ContractPOMasterCommandRepositoryTests(DbFixture fixture) => _fixture = fixture;

        // ContractPONumber is set by the handler; repo only inserts + IncrementDocNoAsync (no-op here).
        private ContractPOMasterCommandRepository CreateRepo(ApplicationDbContext ctx)
        {
            var docSeq = new Mock<IDocumentSequenceLookup>(MockBehavior.Loose);
            docSeq.Setup(d => d.IncrementDocNoAsync(It.IsAny<int>(), It.IsAny<IDbConnection>(), It.IsAny<IDbTransaction>()))
                  .Returns(Task.CompletedTask);
            return new ContractPOMasterCommandRepository(ctx, _fixture.IpMock.Object, docSeq.Object);
        }

        // StatusId → MiscMaster FK → seed one MiscMaster row.
        private async Task<int> EnsureMiscAsync(ApplicationDbContext ctx)
        {
            var mt = new PurchaseManagement.Domain.Entities.MiscTypeMaster
            {
                MiscTypeCode = "CPO_MT", Description = "Contract Type",
                IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
            };
            await ctx.Set<PurchaseManagement.Domain.Entities.MiscTypeMaster>().AddAsync(mt);
            await ctx.SaveChangesAsync();
            var m = new PurchaseManagement.Domain.Entities.MiscMaster
            {
                Code = "CPO_MSC", Description = "Contract Misc", MiscTypeId = mt.Id,
                IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
            };
            await ctx.Set<PurchaseManagement.Domain.Entities.MiscMaster>().AddAsync(m);
            await ctx.SaveChangesAsync();
            return m.Id;
        }

        private static ContractPOHeader BuildEntity(int miscId, string contractNo)
        {
            var now = new DateTimeOffset(2026, 1, 15, 0, 0, 0, TimeSpan.Zero);
            return new ContractPOHeader
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
                IsDeleted = IsDelete.NotDeleted,
                ContractPODetails = new List<ContractPODetail>
                {
                    new()
                    {
                        ItemSno = 1, ItemId = 10, UOMId = 1,
                        ContractQuantity = 100m, ContractRate = 10m, ContractValue = 1000m,
                        UtilizedQuantity = 0m, BalanceQuantity = 100m, UtilizedValue = 0m, BalanceValue = 1000m,
                        GSTPercentage = 18m,
                        IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                    }
                }
            };
        }

        private async Task ClearAsync() => await _fixture.ClearAllTablesAsync();

        [Fact]
        public async Task CreateAsync_Should_Return_Entity_With_Id()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var miscId = await EnsureMiscAsync(ctx);

            var created = await CreateRepo(ctx).CreateAsync(BuildEntity(miscId, "CPO_C1"), 1, CancellationToken.None);

            created.Id.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Header_And_Detail()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var miscId = await EnsureMiscAsync(ctx);

            var created = await CreateRepo(ctx).CreateAsync(BuildEntity(miscId, "CPO_C2"), 1, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.Set<ContractPOHeader>().FirstAsync(x => x.Id == created.Id);
            saved.ContractPONumber.Should().Be("CPO_C2");
            saved.VendorId.Should().Be(100);

            var detailCount = await ctx.Set<ContractPODetail>().CountAsync(d => d.ContractPOHeaderId == created.Id);
            detailCount.Should().Be(1);
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_True_And_Flag()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var miscId = await EnsureMiscAsync(ctx);
            var created = await CreateRepo(ctx).CreateAsync(BuildEntity(miscId, "CPO_D1"), 1, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var result = await CreateRepo(ctx).SoftDeleteAsync(created.Id, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            result.Should().BeTrue();
            var reloaded = await ctx.Set<ContractPOHeader>().IgnoreQueryFilters().FirstAsync(x => x.Id == created.Id);
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
