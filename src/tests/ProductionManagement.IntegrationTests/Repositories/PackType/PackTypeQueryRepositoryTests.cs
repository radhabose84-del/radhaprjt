using Contracts.Interfaces.Validations.SalesManagement;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ProductionManagement.Infrastructure.Data;
using ProductionManagement.Infrastructure.Repositories.PackType;
using ProductionManagement.IntegrationTests.Common;
using static ProductionManagement.Domain.Common.BaseEntity;

namespace ProductionManagement.IntegrationTests.Repositories.PackType
{
    [Collection("DatabaseCollection")]
    public sealed class PackTypeQueryRepositoryTests
    {
        private readonly DbFixture _fixture;
        public PackTypeQueryRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private PackTypeQueryRepository CreateRepo(Mock<IPackTypeSalesValidation>? sales = null)
        {
            if (sales == null)
            {
                sales = new Mock<IPackTypeSalesValidation>(MockBehavior.Loose);
                sales.Setup(s => s.HasLinkedPackTypeAsync(It.IsAny<int>())).ReturnsAsync(false);
                sales.Setup(s => s.HasActivePackTypeAsync(It.IsAny<int>())).ReturnsAsync(false);
            }
            return new PackTypeQueryRepository(new SqlConnection(_fixture.ConnectionString), sales.Object);
        }

        private async Task<int> EnsurePackMaterialAsync()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var t = await ctx.MiscTypeMaster.FirstOrDefaultAsync(x => x.MiscTypeCode == "PT_MAT_T");
            if (t == null)
            {
                t = new Domain.Entities.MiscTypeMaster
                {
                    MiscTypeCode = "PT_MAT_T", Description = "Pack material",
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MiscTypeMaster.AddAsync(t);
                await ctx.SaveChangesAsync();
            }
            var m = await ctx.MiscMaster.FirstOrDefaultAsync(x => x.Code == "PT_MAT");
            if (m == null)
            {
                m = new Domain.Entities.MiscMaster
                {
                    MiscTypeId = t.Id, Code = "PT_MAT", Description = "Material",
                    SortOrder = 1, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MiscMaster.AddAsync(m);
                await ctx.SaveChangesAsync();
            }
            return m.Id;
        }

        private async Task<int> SeedAsync(string code, string? name = null,
            Status active = Status.Active, IsDelete deleted = IsDelete.NotDeleted)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var p = new Domain.Entities.PackType
            {
                PackTypeCode = code, PackTypeName = name ?? code,
                NetWeight = 5m, TareWeight = 1m, GrossWeight = 6m,
                ConesPerBag = 10, ProductionAllowed = true,
                IsActive = active, IsDeleted = deleted
            };
            await ctx.PackType.AddAsync(p);
            await ctx.SaveChangesAsync();
            return p.Id;
        }

        private async Task ClearAsync() =>
            await _fixture.ClearAllTablesAsync();

        [Fact]
        public async Task GetAllAsync_Should_Return_Seeded()
        {
            await ClearAsync();
            await SeedAsync("PTQ1");

            var (rows, total) = await CreateRepo().GetAllAsync(1, 10, null);

            rows.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllAsync_Should_Exclude_SoftDeleted()
        {
            await ClearAsync();
            await SeedAsync("PTQDEL", deleted: IsDelete.Deleted);

            var (_, total) = await CreateRepo().GetAllAsync(1, 10, null);

            total.Should().Be(0);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Matching()
        {
            await ClearAsync();
            var id = await SeedAsync("PTQID");

            var result = await CreateRepo().GetByIdAsync(id);

            result.Should().NotBeNull();
            result!.PackTypeCode.Should().Be("PTQID");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_SoftDeleted()
        {
            await ClearAsync();
            var id = await SeedAsync("PTQSD", deleted: IsDelete.Deleted);

            var result = await CreateRepo().GetByIdAsync(id);

            result.Should().BeNull();
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Return_Active_Matching()
        {
            await ClearAsync();
            await SeedAsync("PTAC1");
            await SeedAsync("PTAC2", active: Status.Inactive);

            var result = await CreateRepo().AutocompleteAsync("PTAC", CancellationToken.None);

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_True_For_Duplicate()
        {
            await ClearAsync();
            await SeedAsync("PTDUP");

            var result = await CreateRepo().AlreadyExistsAsync("PTDUP");

            result.Should().BeTrue();
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Exclude_Self()
        {
            await ClearAsync();
            var id = await SeedAsync("PTSELF");

            var result = await CreateRepo().AlreadyExistsAsync("PTSELF", id);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task NotFoundAsync_Should_Return_True_For_Missing()
        {
            var result = await CreateRepo().NotFoundAsync(9999999);
            result.Should().BeTrue();
        }

        [Fact]
        public async Task PackMaterialExistsAsync_Should_Return_True_For_Active_Material()
        {
            var id = await EnsurePackMaterialAsync();

            var result = await CreateRepo().PackMaterialExistsAsync(id);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task PackMaterialExistsAsync_Should_Return_False_For_Unknown()
        {
            var result = await CreateRepo().PackMaterialExistsAsync(9999999);
            result.Should().BeFalse();
        }

        [Fact]
        public async Task SoftDeleteValidationAsync_Should_Return_False_When_No_Refs()
        {
            await ClearAsync();
            var id = await SeedAsync("PTSDV1");

            var result = await CreateRepo().SoftDeleteValidationAsync(id);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task SoftDeleteValidationAsync_Should_Return_True_When_SalesValidation_Links()
        {
            await ClearAsync();
            var id = await SeedAsync("PTSDV2");
            var sales = new Mock<IPackTypeSalesValidation>(MockBehavior.Loose);
            sales.Setup(s => s.HasLinkedPackTypeAsync(id)).ReturnsAsync(true);

            var result = await CreateRepo(sales: sales).SoftDeleteValidationAsync(id);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task IsPackTypeLinkedAsync_Should_Return_False_When_No_Refs()
        {
            await ClearAsync();
            var id = await SeedAsync("PTLK1");

            var result = await CreateRepo().IsPackTypeLinkedAsync(id);

            result.Should().BeFalse();
        }
    }
}
