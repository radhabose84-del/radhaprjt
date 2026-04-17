using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SalesManagement.Infrastructure.Data;
using SalesManagement.Infrastructure.Repositories.StoTypeMaster;
using SalesManagement.IntegrationTests.Common;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.IntegrationTests.Repositories.StoTypeMaster
{
    [Collection("DatabaseCollection")]
    public sealed class StoTypeMasterQueryRepositoryTests
    {
        private readonly DbFixture _fixture;
        public StoTypeMasterQueryRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private StoTypeMasterQueryRepository CreateRepo() =>
            new(new SqlConnection(_fixture.ConnectionString));

        private async Task<int> EnsureMiscIdAsync(string code = "STMQ_MISC")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var t = await ctx.MiscTypeMaster.FirstOrDefaultAsync(x => x.MiscTypeCode == "STMQ_MT");
            if (t == null)
            {
                t = new SalesManagement.Domain.Entities.MiscTypeMaster
                {
                    MiscTypeCode = "STMQ_MT", Description = "T",
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

        private async Task<(int Pgi, int Gr)> EnsureMovementTypesAsync()
        {
            var miscId = await EnsureMiscIdAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var pgi = await ctx.MovementTypeConfig.FirstOrDefaultAsync(x => x.MovementCode == "QPGI");
            if (pgi == null)
            {
                pgi = new SalesManagement.Domain.Entities.MovementTypeConfig
                {
                    MovementCode = "QPGI", MovementDescription = "PGI",
                    MovementCategoryId = miscId, FromStockTypeId = miscId, ToStockTypeId = miscId,
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MovementTypeConfig.AddAsync(pgi);
                await ctx.SaveChangesAsync();
            }
            var gr = await ctx.MovementTypeConfig.FirstOrDefaultAsync(x => x.MovementCode == "QGR");
            if (gr == null)
            {
                gr = new SalesManagement.Domain.Entities.MovementTypeConfig
                {
                    MovementCode = "QGR", MovementDescription = "GR",
                    MovementCategoryId = miscId, FromStockTypeId = miscId, ToStockTypeId = miscId,
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MovementTypeConfig.AddAsync(gr);
                await ctx.SaveChangesAsync();
            }
            return (pgi.Id, gr.Id);
        }

        private async Task<int> SeedAsync(
            string code, string? name = null,
            Status active = Status.Active, IsDelete deleted = IsDelete.NotDeleted)
        {
            var (pgiId, grId) = await EnsureMovementTypesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var s = new SalesManagement.Domain.Entities.StoTypeMaster
            {
                StoTypeCode = code, StoTypeName = name ?? code, Description = "desc",
                PgiMovementTypeId = pgiId, GrMovementTypeId = grId,
                IsActive = active, IsDeleted = deleted
            };
            await ctx.StoTypeMaster.AddAsync(s);
            await ctx.SaveChangesAsync();
            return s.Id;
        }

        private async Task ClearAsync() =>
            await _fixture.ClearAllTablesAsync();

        [Fact]
        public async Task GetAllAsync_Should_Return_Seeded()
        {
            await ClearAsync();
            await SeedAsync("STMQ1");

            var (rows, total) = await CreateRepo().GetAllAsync(1, 10, null);

            rows.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllAsync_Should_Exclude_SoftDeleted()
        {
            await ClearAsync();
            await SeedAsync("STMQDEL", deleted: IsDelete.Deleted);

            var (_, total) = await CreateRepo().GetAllAsync(1, 10, null);

            total.Should().Be(0);
        }

        [Fact]
        public async Task GetAllAsync_Should_Filter_By_SearchTerm()
        {
            await ClearAsync();
            await SeedAsync("STMQ_UNIQ");
            await SeedAsync("STMQ_OTHER");

            var (rows, _) = await CreateRepo().GetAllAsync(1, 10, "STMQ_UNIQ");

            rows.Should().HaveCount(1);
            rows[0].StoTypeCode.Should().Be("STMQ_UNIQ");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Matching()
        {
            await ClearAsync();
            var id = await SeedAsync("STMQ_GBI");

            var result = await CreateRepo().GetByIdAsync(id);

            result.Should().NotBeNull();
            result!.StoTypeCode.Should().Be("STMQ_GBI");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_SoftDeleted()
        {
            await ClearAsync();
            var id = await SeedAsync("STMQ_GSD", deleted: IsDelete.Deleted);

            var result = await CreateRepo().GetByIdAsync(id);

            result.Should().BeNull();
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Return_Active_Matching()
        {
            await ClearAsync();
            await SeedAsync("STMQ_AC1");
            await SeedAsync("STMQ_AC2", active: Status.Inactive);

            var result = await CreateRepo().AutocompleteAsync("STMQ_AC", CancellationToken.None);

            result.Should().HaveCount(1);
            result[0].StoTypeCode.Should().Be("STMQ_AC1");
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_True_For_Duplicate()
        {
            await ClearAsync();
            await SeedAsync("STMQ_DUP");

            var result = await CreateRepo().AlreadyExistsAsync("STMQ_DUP");

            result.Should().BeTrue();
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Exclude_Self()
        {
            await ClearAsync();
            var id = await SeedAsync("STMQ_SELF");

            var result = await CreateRepo().AlreadyExistsAsync("STMQ_SELF", id);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task NotFoundAsync_Should_Return_True_For_Missing()
        {
            var result = await CreateRepo().NotFoundAsync(9999999);
            result.Should().BeTrue();
        }

        [Fact]
        public async Task MovementTypeExistsAsync_Should_Return_True_For_Active()
        {
            var (pgiId, _) = await EnsureMovementTypesAsync();

            var result = await CreateRepo().MovementTypeExistsAsync(pgiId);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task MovementTypeExistsAsync_Should_Return_False_For_Unknown()
        {
            var result = await CreateRepo().MovementTypeExistsAsync(9999999);
            result.Should().BeFalse();
        }

        [Fact]
        public async Task SoftDeleteValidationAsync_Should_Return_False_When_No_Refs()
        {
            await ClearAsync();
            var id = await SeedAsync("STMQ_SDV");

            var result = await CreateRepo().SoftDeleteValidationAsync(id);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task IsStoTypeMasterLinkedAsync_Should_Return_False_When_No_Refs()
        {
            await ClearAsync();
            var id = await SeedAsync("STMQ_LK");

            var result = await CreateRepo().IsStoTypeMasterLinkedAsync(id);

            result.Should().BeFalse();
        }
    }
}
