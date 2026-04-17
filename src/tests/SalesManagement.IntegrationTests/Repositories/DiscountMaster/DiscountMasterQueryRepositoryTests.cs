using Contracts.Interfaces.Lookups.Purchase;
using Contracts.Interfaces.Lookups.Users;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SalesManagement.Infrastructure.Data;
using SalesManagement.Infrastructure.Repositories.DiscountMaster;
using SalesManagement.IntegrationTests.Common;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.IntegrationTests.Repositories.DiscountMaster
{
    [Collection("DatabaseCollection")]
    public sealed class DiscountMasterQueryRepositoryTests
    {
        private readonly DbFixture _fixture;
        public DiscountMasterQueryRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private DiscountMasterQueryRepository CreateRepo(
            Mock<IPaymentTermLookup>? paymentTerm = null,
            Mock<ICurrencyLookup>? currency = null)
        {
            paymentTerm ??= new Mock<IPaymentTermLookup>(MockBehavior.Loose);
            currency ??= new Mock<ICurrencyLookup>(MockBehavior.Loose);
            return new DiscountMasterQueryRepository(
                new SqlConnection(_fixture.ConnectionString),
                paymentTerm.Object, currency.Object);
        }

        private async Task<int> EnsureMiscIdAsync(string code = "DMQ_M")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var t = await ctx.MiscTypeMaster.FirstOrDefaultAsync(x => x.MiscTypeCode == "DMQ_T");
            if (t == null)
            {
                t = new SalesManagement.Domain.Entities.MiscTypeMaster
                {
                    MiscTypeCode = "DMQ_T", Description = "T",
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

        private async Task<int> SeedAsync(
            string name, string code,
            Status active = Status.Active, IsDelete deleted = IsDelete.NotDeleted)
        {
            var miscId = await EnsureMiscIdAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var d = new SalesManagement.Domain.Entities.DiscountMaster
            {
                DiscountCode = code,
                DiscountName = name,
                TriggerEventId = miscId,
                DiscountBasisId = miscId,
                ExecutionTypeId = miscId,
                Priority = 1,
                ValueTypeId = miscId,
                SlabTypeId = miscId,
                IsActive = active,
                IsDeleted = deleted
            };
            await ctx.DiscountMaster.AddAsync(d);
            await ctx.SaveChangesAsync();
            return d.Id;
        }

        private async Task ClearAsync() =>
            await _fixture.ClearAllTablesAsync();

        [Fact]
        public async Task GetAllAsync_Should_Return_Seeded()
        {
            await ClearAsync();
            await SeedAsync("DQ1", "DC00001");

            var (rows, total) = await CreateRepo().GetAllAsync(1, 10, null);

            rows.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllAsync_Should_Exclude_SoftDeleted()
        {
            await ClearAsync();
            await SeedAsync("DQDEL", "DC00099", deleted: IsDelete.Deleted);

            var (_, total) = await CreateRepo().GetAllAsync(1, 10, null);

            total.Should().Be(0);
        }

        [Fact]
        public async Task GetAllAsync_Should_Filter_By_SearchTerm()
        {
            await ClearAsync();
            await SeedAsync("DQ_UNIQ", "DC00010");
            await SeedAsync("DQ_OTHER", "DC00011");

            var (rows, _) = await CreateRepo().GetAllAsync(1, 10, "DQ_UNIQ");

            rows.Should().HaveCount(1);
            rows[0].DiscountName.Should().Be("DQ_UNIQ");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Matching()
        {
            await ClearAsync();
            var id = await SeedAsync("DQ_GBI", "DC00020");

            var result = await CreateRepo().GetByIdAsync(id);

            result.Should().NotBeNull();
            result!.DiscountName.Should().Be("DQ_GBI");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_NotFound()
        {
            var result = await CreateRepo().GetByIdAsync(9999999);
            result.Should().BeNull();
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Return_Active_Matching()
        {
            await ClearAsync();
            await SeedAsync("DQ_AC1", "DC00030");
            await SeedAsync("DQ_AC2", "DC00031", active: Status.Inactive);

            var result = await CreateRepo().AutocompleteAsync("DQ_AC", CancellationToken.None);

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_True_For_Duplicate_Name()
        {
            await ClearAsync();
            await SeedAsync("DQ_DUP", "DC00040");

            var result = await CreateRepo().AlreadyExistsAsync("DQ_DUP");

            result.Should().BeTrue();
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Exclude_Self()
        {
            await ClearAsync();
            var id = await SeedAsync("DQ_SELF", "DC00050");

            var result = await CreateRepo().AlreadyExistsAsync("DQ_SELF", id);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task NotFoundAsync_Should_Return_True_For_Missing()
        {
            var result = await CreateRepo().NotFoundAsync(9999999);
            result.Should().BeTrue();
        }

        [Fact]
        public async Task MiscMasterExistsAsync_Should_Return_True_For_Active()
        {
            var id = await EnsureMiscIdAsync();

            var result = await CreateRepo().MiscMasterExistsAsync(id);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task SalesGroupExistsAsync_Should_Return_False_For_Unknown()
        {
            var result = await CreateRepo().SalesGroupExistsAsync(9999999);
            result.Should().BeFalse();
        }

        [Fact]
        public async Task SoftDeleteValidationAsync_Should_Return_False_When_No_Refs()
        {
            await ClearAsync();
            var id = await SeedAsync("DQ_SDV", "DC00060");

            var result = await CreateRepo().SoftDeleteValidationAsync(id);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task IsDiscountMasterLinkedAsync_Should_Return_False_When_No_Refs()
        {
            await ClearAsync();
            var id = await SeedAsync("DQ_LK", "DC00070");

            var result = await CreateRepo().IsDiscountMasterLinkedAsync(id);

            result.Should().BeFalse();
        }
    }
}
