using Contracts.Dtos.Lookups.Finance;
using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Finance;
using Contracts.Interfaces.Lookups.Users;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SalesManagement.Application.Common.Constants;
using SalesManagement.Infrastructure.Repositories.SalesOrderTypeMaster;
using SalesManagement.IntegrationTests.Common;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.IntegrationTests.Repositories.SalesOrderTypeMaster
{
    [Collection("DatabaseCollection")]
    public sealed class SalesOrderTypeMasterQueryRepositoryTests
    {
        private readonly DbFixture _fixture;
        public SalesOrderTypeMasterQueryRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private SalesOrderTypeMasterQueryRepository CreateRepo()
        {
            var tx = new Mock<ITransactionTypeLookup>(MockBehavior.Loose);
            tx.Setup(l => l.GetByIdsAsync(It.IsAny<IEnumerable<int>>()))
                .ReturnsAsync(new List<TransactionTypeLookupDto>());

            var ccy = new Mock<ICurrencyLookup>(MockBehavior.Loose);
            ccy.Setup(l => l.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<CurrencyLookupDto>());

            return new SalesOrderTypeMasterQueryRepository(
                new SqlConnection(_fixture.ConnectionString), tx.Object, ccy.Object);
        }

        private async Task<int> EnsureSoTypeIdAsync(string code = MiscMasterCodes.SO_NORMAL)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var t = await ctx.MiscTypeMaster.FirstOrDefaultAsync(x => x.MiscTypeCode == MiscMasterCodes.SOTM_TYPE_MISCTYPE);
            if (t == null)
            {
                t = new SalesManagement.Domain.Entities.MiscTypeMaster
                {
                    MiscTypeCode = MiscMasterCodes.SOTM_TYPE_MISCTYPE,
                    Description = "Sales Order Type",
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MiscTypeMaster.AddAsync(t);
                await ctx.SaveChangesAsync();
            }
            var m = await ctx.MiscMaster.FirstOrDefaultAsync(x => x.Code == code);
            if (m == null)
            {
                m = new SalesManagement.Domain.Entities.MiscMaster
                {
                    MiscTypeId = t.Id,
                    Code = code,
                    Description = code,
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MiscMaster.AddAsync(m);
                await ctx.SaveChangesAsync();
            }
            return m.Id;
        }

        private async Task<int> SeedAsync(
            string typeName, int taxTypeId = 1, int? soTypeId = null,
            Status active = Status.Active, IsDelete deleted = IsDelete.NotDeleted)
        {
            soTypeId ??= await EnsureSoTypeIdAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var s = new SalesManagement.Domain.Entities.SalesOrderTypeMaster
            {
                SoTypeId = soTypeId.Value,
                TaxTypeId = taxTypeId,
                TypeName = typeName,
                Description = "desc",
                IsActive = active,
                IsDeleted = deleted
            };
            await ctx.SalesOrderTypeMaster.AddAsync(s);
            await ctx.SaveChangesAsync();

            // IsActive has HasDefaultValue(Status.Active); since Status.Inactive is the CLR default (0),
            // EF omits it on INSERT and the DB default (Active) wins. Force it via a follow-up update.
            if (active == Status.Inactive)
            {
                s.IsActive = Status.Inactive;
                await ctx.SaveChangesAsync();
            }
            return s.Id;
        }

        private async Task ClearAsync() => await _fixture.ClearAllTablesAsync();

        [Fact]
        public async Task GetAllAsync_Should_Return_Seeded()
        {
            await ClearAsync();
            await SeedAsync("Normal Q1");

            var (rows, total) = await CreateRepo().GetAllAsync(1, 10, null);

            rows.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllAsync_Should_Exclude_SoftDeleted()
        {
            await ClearAsync();
            await SeedAsync("Normal QDel", deleted: IsDelete.Deleted);

            var (_, total) = await CreateRepo().GetAllAsync(1, 10, null);

            total.Should().Be(0);
        }

        [Fact]
        public async Task GetAllAsync_Should_Filter_By_SearchTerm()
        {
            await ClearAsync();
            await SeedAsync("UNIQ Alpha", taxTypeId: 1);
            await SeedAsync("OTHER Beta", taxTypeId: 2);

            var (rows, _) = await CreateRepo().GetAllAsync(1, 10, "UNIQ");

            rows.Should().HaveCount(1);
            rows[0].TypeName.Should().Be("UNIQ Alpha");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Matching()
        {
            await ClearAsync();
            var id = await SeedAsync("Normal GBI");

            var result = await CreateRepo().GetByIdAsync(id);

            result.Should().NotBeNull();
            result!.TypeName.Should().Be("Normal GBI");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_SoftDeleted()
        {
            await ClearAsync();
            var id = await SeedAsync("Normal GSD", deleted: IsDelete.Deleted);

            var result = await CreateRepo().GetByIdAsync(id);

            result.Should().BeNull();
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Return_Active_Matching()
        {
            await ClearAsync();
            await SeedAsync("AC Active", taxTypeId: 1);
            await SeedAsync("AC Inactive", taxTypeId: 2, active: Status.Inactive);

            var result = await CreateRepo().AutocompleteAsync("AC", CancellationToken.None);

            result.Should().HaveCount(1);
            result[0].TypeName.Should().Be("AC Active");
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_True_For_Duplicate()
        {
            await ClearAsync();
            var soTypeId = await EnsureSoTypeIdAsync();
            await SeedAsync("Normal Dup", taxTypeId: 7, soTypeId: soTypeId);

            var result = await CreateRepo().AlreadyExistsAsync(soTypeId, 7);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Exclude_Self()
        {
            await ClearAsync();
            var soTypeId = await EnsureSoTypeIdAsync();
            var id = await SeedAsync("Normal Self", taxTypeId: 8, soTypeId: soTypeId);

            var result = await CreateRepo().AlreadyExistsAsync(soTypeId, 8, id);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task NotFoundAsync_Should_Return_True_For_Missing()
        {
            var result = await CreateRepo().NotFoundAsync(9999999);
            result.Should().BeTrue();
        }

        [Fact]
        public async Task IsValidSoTypeAsync_Should_Return_True_For_Valid()
        {
            var soTypeId = await EnsureSoTypeIdAsync();

            var result = await CreateRepo().IsValidSoTypeAsync(soTypeId);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task IsValidSoTypeAsync_Should_Return_False_For_Unknown()
        {
            var result = await CreateRepo().IsValidSoTypeAsync(9999999);
            result.Should().BeFalse();
        }

        [Fact]
        public async Task GetSoTypeCodeAsync_Should_Return_Code()
        {
            var soTypeId = await EnsureSoTypeIdAsync();

            var code = await CreateRepo().GetSoTypeCodeAsync(soTypeId);

            code.Should().Be(MiscMasterCodes.SO_NORMAL);
        }

        [Fact]
        public async Task GetSoTypeIdByRowIdAsync_Should_Return_SoTypeId()
        {
            await ClearAsync();
            var soTypeId = await EnsureSoTypeIdAsync();
            var id = await SeedAsync("Normal Row", taxTypeId: 9, soTypeId: soTypeId);

            var result = await CreateRepo().GetSoTypeIdByRowIdAsync(id);

            result.Should().Be(soTypeId);
        }
    }
}
