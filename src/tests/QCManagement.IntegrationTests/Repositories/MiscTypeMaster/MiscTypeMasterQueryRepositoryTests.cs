using Microsoft.Data.SqlClient;
using QCManagement.Infrastructure.Data;
using QCManagement.Infrastructure.Repositories.MiscTypeMaster;
using QCManagement.IntegrationTests.Common;
using static QCManagement.Domain.Common.BaseEntity;

namespace QCManagement.IntegrationTests.Repositories.MiscTypeMaster
{
    [Collection("DatabaseCollection")]
    public sealed class MiscTypeMasterQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public MiscTypeMasterQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private MiscTypeMasterQueryRepository CreateQueryRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new MiscTypeMasterQueryRepository(conn);
        }

        private MiscTypeMasterCommandRepository CreateCommandRepo(ApplicationDbContext ctx)
            => new MiscTypeMasterCommandRepository(ctx);

        private Domain.Entities.MiscTypeMaster BuildEntity(
            string code = "QP_GROUP",
            string description = "Quality Parameter Group",
            bool isActive = true) =>
            new Domain.Entities.MiscTypeMaster
            {
                MiscTypeCode = code,
                Description = description,
                IsActive = isActive ? Status.Active : Status.Inactive,
                IsDeleted = IsDelete.NotDeleted
            };

        private async Task ClearTableAsync() =>
            await _fixture.ClearTablesAsync("QC.QualityParameter", "QC.MiscMaster", "QC.MiscTypeMaster");

        private async Task<int> SeedEntityAsync(Domain.Entities.MiscTypeMaster entity)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            return await CreateCommandRepo(ctx).CreateAsync(entity);
        }

        [Fact]
        public async Task GetAllAsync_Should_Return_Seeded_Records()
        {
            await ClearTableAsync();
            await SeedEntityAsync(BuildEntity("QP_GROUP", "Group"));
            await SeedEntityAsync(BuildEntity("QP_DATATYPE", "Data Type"));

            var (data, totalCount) = await CreateQueryRepo().GetAllAsync(1, 10, null);

            totalCount.Should().Be(2);
            data.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetAllAsync_Should_Support_Pagination()
        {
            await ClearTableAsync();
            for (int i = 1; i <= 5; i++)
                await SeedEntityAsync(BuildEntity($"QP_TYPE_{(char)('A' + i - 1)}", $"Type {i}"));

            var (page1, total) = await CreateQueryRepo().GetAllAsync(1, 3, null);
            var (page2, _) = await CreateQueryRepo().GetAllAsync(2, 3, null);

            total.Should().Be(5);
            page1.Should().HaveCount(3);
            page2.Should().HaveCount(2);

            var page1Codes = page1.Select(x => x.MiscTypeCode).ToList();
            var page2Codes = page2.Select(x => x.MiscTypeCode).ToList();
            page1Codes.Should().NotIntersectWith(page2Codes);
        }

        [Fact]
        public async Task GetAllAsync_Should_Filter_By_SearchTerm()
        {
            await ClearTableAsync();
            await SeedEntityAsync(BuildEntity("QP_ALPHA", "Alpha Type"));
            await SeedEntityAsync(BuildEntity("QP_BETA", "Beta Type"));

            var (data, totalCount) = await CreateQueryRepo().GetAllAsync(1, 10, "ALPHA");

            totalCount.Should().Be(1);
            data.Should().HaveCount(1);
            data[0].MiscTypeCode.Should().Be("QP_ALPHA");
        }

        [Fact]
        public async Task GetAllAsync_Should_Not_Return_SoftDeleted_Records()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync(BuildEntity("QP_DEL_TYPE", "Deleted Type"));

            await using var ctx = _fixture.CreateFreshDbContext();
            await CreateCommandRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var (data, totalCount) = await CreateQueryRepo().GetAllAsync(1, 10, null);

            data.Should().NotContain(x => x.MiscTypeCode == "QP_DEL_TYPE");
            totalCount.Should().Be(0);
        }

        [Fact]
        public async Task GetAllAsync_Should_Return_Empty_WhenNoData()
        {
            await ClearTableAsync();

            var (data, totalCount) = await CreateQueryRepo().GetAllAsync(1, 10, null);

            data.Should().BeEmpty();
            totalCount.Should().Be(0);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_DTO()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync(BuildEntity("QP_BYID", "ById Type"));

            var dto = await CreateQueryRepo().GetByIdAsync(id);

            dto.Should().NotBeNull();
            dto!.Id.Should().Be(id);
            dto.MiscTypeCode.Should().Be("QP_BYID");
            dto.Description.Should().Be("ById Type");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_WhenNotFound()
        {
            await ClearTableAsync();

            var dto = await CreateQueryRepo().GetByIdAsync(99999);

            dto.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_WhenSoftDeleted()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync(BuildEntity("QP_SDEL", "Soft Deleted"));

            await using var ctx = _fixture.CreateFreshDbContext();
            await CreateCommandRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var dto = await CreateQueryRepo().GetByIdAsync(id);

            dto.Should().BeNull();
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_True_WhenCodeExists()
        {
            await ClearTableAsync();
            await SeedEntityAsync(BuildEntity("QP_EXISTS"));

            var result = await CreateQueryRepo().AlreadyExistsAsync("QP_EXISTS");

            result.Should().BeTrue();
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_False_WhenCodeDoesNotExist()
        {
            await ClearTableAsync();

            var result = await CreateQueryRepo().AlreadyExistsAsync("QP_MISSING");

            result.Should().BeFalse();
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_False_WhenExcludedId_Matches()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync(BuildEntity("QP_EXCL"));

            var result = await CreateQueryRepo().AlreadyExistsAsync("QP_EXCL", id: id);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_True_WhenDifferentRecord_HasSameCode()
        {
            await ClearTableAsync();
            await SeedEntityAsync(BuildEntity("QP_DUP_A"));
            var idB = await SeedEntityAsync(BuildEntity("QP_DUP_B"));

            var result = await CreateQueryRepo().AlreadyExistsAsync("QP_DUP_A", id: idB);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_False_ForSoftDeleted_Records()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync(BuildEntity("QP_DEL_DUP"));

            await using var ctx = _fixture.CreateFreshDbContext();
            await CreateCommandRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var result = await CreateQueryRepo().AlreadyExistsAsync("QP_DEL_DUP");

            result.Should().BeFalse();
        }

        [Fact]
        public async Task NotFoundAsync_Should_Return_False_WhenEntityExists()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync(BuildEntity());

            var result = await CreateQueryRepo().NotFoundAsync(id);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task NotFoundAsync_Should_Return_True_WhenEntityDoesNotExist()
        {
            await ClearTableAsync();

            var result = await CreateQueryRepo().NotFoundAsync(99999);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Return_Matching_Results()
        {
            await ClearTableAsync();
            await SeedEntityAsync(BuildEntity("QP_ACM_ONE", "Acme One"));
            await SeedEntityAsync(BuildEntity("QP_ACM_TWO", "Acme Two"));
            await SeedEntityAsync(BuildEntity("QP_XYZ", "XYZ"));

            var results = await CreateQueryRepo().AutocompleteAsync("ACM", CancellationToken.None);

            results.Should().HaveCount(2);
            results.Select(r => r.MiscTypeCode).Should().Contain(new[] { "QP_ACM_ONE", "QP_ACM_TWO" });
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Not_Return_Inactive_Records()
        {
            await ClearTableAsync();
            await SeedEntityAsync(BuildEntity("QP_ACTV", "Active Type", isActive: true));
            await SeedEntityAsync(BuildEntity("QP_INAC", "Inactive Type", isActive: false));

            var results = await CreateQueryRepo().AutocompleteAsync("Type", CancellationToken.None);

            results.Should().NotContain(r => r.MiscTypeCode == "QP_INAC");
            results.Should().Contain(r => r.MiscTypeCode == "QP_ACTV");
        }

        [Fact]
        public async Task SoftDeleteValidationAsync_Should_Return_False_WhenNoDependentMiscMaster()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync(BuildEntity("QP_NO_DEP"));

            var result = await CreateQueryRepo().SoftDeleteValidationAsync(id);

            result.Should().BeFalse();
        }
    }
}
