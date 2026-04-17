using Contracts.Dtos.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Inventory;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ProductionManagement.Infrastructure.Data;
using ProductionManagement.Infrastructure.Repositories.CountMaster;
using ProductionManagement.IntegrationTests.Common;
using static ProductionManagement.Domain.Common.BaseEntity;

namespace ProductionManagement.IntegrationTests.Repositories.CountMaster
{
    [Collection("DatabaseCollection")]
    public sealed class CountMasterQueryRepositoryTests
    {
        private readonly DbFixture _fixture;
        public CountMasterQueryRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private CountMasterQueryRepository CreateRepo(Mock<IUOMLookup>? uomLookup = null)
        {
            if (uomLookup == null)
            {
                uomLookup = new Mock<IUOMLookup>(MockBehavior.Loose);
                uomLookup.Setup(l => l.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((IReadOnlyList<UOMLookupDto>)new List<UOMLookupDto>());
            }
            return new CountMasterQueryRepository(new SqlConnection(_fixture.ConnectionString), uomLookup.Object);
        }

        private async Task<int> EnsureMiscIdAsync()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var t = await ctx.MiscTypeMaster.FirstOrDefaultAsync(x => x.MiscTypeCode == "CMQ_TYP");
            if (t == null)
            {
                t = new Domain.Entities.MiscTypeMaster
                {
                    MiscTypeCode = "CMQ_TYP", Description = "type",
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MiscTypeMaster.AddAsync(t);
                await ctx.SaveChangesAsync();
            }
            var m = await ctx.MiscMaster.FirstOrDefaultAsync(x => x.Code == "CMQ_MM");
            if (m == null)
            {
                m = new Domain.Entities.MiscMaster
                {
                    MiscTypeId = t.Id, Code = "CMQ_MM", Description = "MM",
                    SortOrder = 1, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MiscMaster.AddAsync(m);
                await ctx.SaveChangesAsync();
            }
            return m.Id;
        }

        private async Task<int> SeedAsync(string code, Status active = Status.Active, IsDelete deleted = IsDelete.NotDeleted)
        {
            var miscId = await EnsureMiscIdAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var c = new Domain.Entities.CountMaster
            {
                CountCode = code,
                CountValue = 30m,
                ShortName = "S",
                CountTypeId = miscId,
                CountCategoryId = miscId,
                CountDescription = "Cot " + code,
                UOMId = 1,
                IsActive = active,
                IsDeleted = deleted
            };
            await ctx.CountMaster.AddAsync(c);
            await ctx.SaveChangesAsync();
            return c.Id;
        }

        private async Task ClearAsync() =>
            await _fixture.ClearAllTablesAsync();

        [Fact]
        public async Task GetAllAsync_Should_Return_Seeded()
        {
            await ClearAsync();
            await SeedAsync("CMQ1");

            var (rows, total) = await CreateRepo().GetAllAsync(1, 10, null);

            rows.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllAsync_Should_Exclude_SoftDeleted()
        {
            await ClearAsync();
            await SeedAsync("CMQDEL", deleted: IsDelete.Deleted);

            var (_, total) = await CreateRepo().GetAllAsync(1, 10, null);

            total.Should().Be(0);
        }

        [Fact]
        public async Task GetAllAsync_Should_Enrich_With_UomLookup()
        {
            await ClearAsync();
            await SeedAsync("CMQU");
            var uomLookup = new Mock<IUOMLookup>(MockBehavior.Loose);
            uomLookup.Setup(l => l.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<UOMLookupDto>)new List<UOMLookupDto>
                {
                    new() { Id = 1, Code = "KG", UOMName = "Kilogram" }
                });

            var (rows, _) = await CreateRepo(uomLookup: uomLookup).GetAllAsync(1, 10, null);

            rows[0].UOMCode.Should().Be("KG");
            rows[0].UOMName.Should().Be("Kilogram");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Matching()
        {
            await ClearAsync();
            var id = await SeedAsync("CMQID");

            var result = await CreateRepo().GetByIdAsync(id);

            result.Should().NotBeNull();
            result!.CountCode.Should().Be("CMQID");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_SoftDeleted()
        {
            await ClearAsync();
            var id = await SeedAsync("CMQSD", deleted: IsDelete.Deleted);

            var result = await CreateRepo().GetByIdAsync(id);

            result.Should().BeNull();
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Return_Active_Matching()
        {
            await ClearAsync();
            await SeedAsync("CMAC1");
            await SeedAsync("CMAC2", active: Status.Inactive);

            var result = await CreateRepo().AutocompleteAsync("CMAC", CancellationToken.None);

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetNextCountCodeAsync_Should_Return_One_When_Empty()
        {
            await ClearAsync();

            var result = await CreateRepo().GetNextCountCodeAsync();

            result.Should().Be("1");
        }

        [Fact]
        public async Task GetNextCountCodeAsync_Should_Return_Max_Plus_One()
        {
            await ClearAsync();
            await SeedAsync("17");

            var result = await CreateRepo().GetNextCountCodeAsync();

            result.Should().Be("18");
        }

        [Fact]
        public async Task NotFoundAsync_Should_Return_True_For_Missing()
        {
            var result = await CreateRepo().NotFoundAsync(9999999);
            result.Should().BeTrue();
        }

        [Fact]
        public async Task CountTypeExistsAsync_Should_Return_True_For_Active_Misc()
        {
            var miscId = await EnsureMiscIdAsync();

            var result = await CreateRepo().CountTypeExistsAsync(miscId);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task CountCategoryExistsAsync_Should_Return_False_For_Unknown()
        {
            var result = await CreateRepo().CountCategoryExistsAsync(9999999);
            result.Should().BeFalse();
        }
    }
}
