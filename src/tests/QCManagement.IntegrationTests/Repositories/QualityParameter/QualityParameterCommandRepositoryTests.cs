using Microsoft.EntityFrameworkCore;
using QCManagement.Infrastructure.Data;
using QCManagement.Infrastructure.Repositories.MiscMaster;
using QCManagement.Infrastructure.Repositories.MiscTypeMaster;
using QCManagement.Infrastructure.Repositories.QualityParameter;
using QCManagement.IntegrationTests.Common;
using static QCManagement.Domain.Common.BaseEntity;

namespace QCManagement.IntegrationTests.Repositories.QualityParameter
{
    [Collection("DatabaseCollection")]
    public sealed class QualityParameterCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public QualityParameterCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private QualityParameterCommandRepository CreateRepo(ApplicationDbContext ctx)
            => new QualityParameterCommandRepository(ctx);

        private async Task ClearAllAsync() =>
            await _fixture.ClearTablesAsync("QC.QualityTemplateParameter", "QC.QualityTemplate", "QC.QualityParameter", "QC.MiscMaster", "QC.MiscTypeMaster");

        private async Task<(int groupId, int dataTypeId, int validationTypeId)> SeedDependenciesAsync()
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            var groupType = await new MiscTypeMasterCommandRepository(ctx).CreateAsync(
                new Domain.Entities.MiscTypeMaster { MiscTypeCode = "QP_GROUP", Description = "Group", IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted });
            var dataType = await new MiscTypeMasterCommandRepository(ctx).CreateAsync(
                new Domain.Entities.MiscTypeMaster { MiscTypeCode = "QP_DATATYPE", Description = "Data Type", IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted });
            var validationType = await new MiscTypeMasterCommandRepository(ctx).CreateAsync(
                new Domain.Entities.MiscTypeMaster { MiscTypeCode = "QP_VALIDATION", Description = "Validation Type", IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted });

            var miscRepo = new MiscMasterCommandRepository(ctx);
            var groupId = await miscRepo.CreateAsync(
                new Domain.Entities.MiscMaster { MiscTypeId = groupType, Code = "MEC", Description = "Mechanical", SortOrder = 1, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted });
            var dataTypeId = await miscRepo.CreateAsync(
                new Domain.Entities.MiscMaster { MiscTypeId = dataType, Code = "DEC", Description = "Decimal", SortOrder = 1, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted });
            var validationTypeId = await miscRepo.CreateAsync(
                new Domain.Entities.MiscMaster { MiscTypeId = validationType, Code = "RNG", Description = "Range", SortOrder = 1, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted });

            return (groupId, dataTypeId, validationTypeId);
        }

        private Domain.Entities.QualityParameter BuildEntity(int groupId, int dataTypeId, int validationTypeId, string code = "QP-000001", string name = "Yarn Tensile Strength") =>
            new Domain.Entities.QualityParameter
            {
                ParameterCode = code,
                ParameterName = name,
                ParameterGroupId = groupId,
                DataTypeId = dataTypeId,
                UnitId = null,
                ValidationTypeId = validationTypeId,
                Description = "Test description",
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };

        [Fact]
        public async Task CreateAsync_Should_Return_NewId()
        {
            await ClearAllAsync();
            var (g, d, v) = await SeedDependenciesAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            var newId = await CreateRepo(ctx).CreateAsync(BuildEntity(g, d, v));

            newId.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields()
        {
            await ClearAllAsync();
            var (g, d, v) = await SeedDependenciesAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            var id = await CreateRepo(ctx).CreateAsync(BuildEntity(g, d, v, "QP-000001", "Tensile"));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.QualityParameter.FirstOrDefaultAsync(x => x.Id == id);
            saved.Should().NotBeNull();
            saved!.ParameterCode.Should().Be("QP-000001");
            saved.ParameterName.Should().Be("Tensile");
            saved.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public async Task CreateAsync_Should_Populate_Audit_Fields()
        {
            await ClearAllAsync();
            var (g, d, v) = await SeedDependenciesAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            var id = await CreateRepo(ctx).CreateAsync(BuildEntity(g, d, v));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.QualityParameter.FirstOrDefaultAsync(x => x.Id == id);
            saved!.CreatedBy.Should().Be(1);
            saved.CreatedByName.Should().Be("test-user");
            saved.CreatedIP.Should().Be("127.0.0.1");
            saved.CreatedDate.Should().NotBeNull();
        }

        [Fact]
        public async Task UpdateAsync_Should_Persist_Mutable_Fields()
        {
            await ClearAllAsync();
            var (g, d, v) = await SeedDependenciesAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            var id = await CreateRepo(ctx).CreateAsync(BuildEntity(g, d, v, name: "Original"));
            ctx.ChangeTracker.Clear();

            await CreateRepo(ctx).UpdateAsync(new Domain.Entities.QualityParameter
            {
                Id = id,
                ParameterName = "Updated Name",
                ParameterGroupId = g,
                UnitId = null,
                Description = "Updated Desc",
                IsActive = Status.Inactive
            });
            ctx.ChangeTracker.Clear();

            var saved = await ctx.QualityParameter.FirstOrDefaultAsync(x => x.Id == id);
            saved!.ParameterName.Should().Be("Updated Name");
            saved.Description.Should().Be("Updated Desc");
            saved.IsActive.Should().Be(Status.Inactive);
        }

        [Fact]
        public async Task UpdateAsync_Should_Not_Change_Immutable_Fields()
        {
            await ClearAllAsync();
            var (g, d, v) = await SeedDependenciesAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            var id = await CreateRepo(ctx).CreateAsync(BuildEntity(g, d, v, code: "QP-000001"));
            ctx.ChangeTracker.Clear();

            await CreateRepo(ctx).UpdateAsync(new Domain.Entities.QualityParameter
            {
                Id = id,
                ParameterName = "Different",
                ParameterGroupId = g,
                UnitId = null,
                Description = "Different",
                IsActive = Status.Active
            });
            ctx.ChangeTracker.Clear();

            var saved = await ctx.QualityParameter.FirstOrDefaultAsync(x => x.Id == id);
            saved!.ParameterCode.Should().Be("QP-000001");
            saved.DataTypeId.Should().Be(d);
            saved.ValidationTypeId.Should().Be(v);
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_Zero_WhenNotFound()
        {
            await ClearAllAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var result = await CreateRepo(ctx).UpdateAsync(new Domain.Entities.QualityParameter
            {
                Id = 99999,
                ParameterName = "Ghost",
                ParameterGroupId = 1,
                UnitId = null,
                IsActive = Status.Active
            });

            result.Should().Be(0);
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_True_WhenExists()
        {
            await ClearAllAsync();
            var (g, d, v) = await SeedDependenciesAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            var id = await CreateRepo(ctx).CreateAsync(BuildEntity(g, d, v));
            ctx.ChangeTracker.Clear();

            var result = await CreateRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_SetIsDeleted_Flag()
        {
            await ClearAllAsync();
            var (g, d, v) = await SeedDependenciesAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            var id = await CreateRepo(ctx).CreateAsync(BuildEntity(g, d, v));
            ctx.ChangeTracker.Clear();

            await CreateRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.QualityParameter.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Id == id);
            saved!.IsDeleted.Should().Be(IsDelete.Deleted);
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_False_WhenNotFound()
        {
            await ClearAllAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var result = await CreateRepo(ctx).SoftDeleteAsync(99999, CancellationToken.None);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task GetMaxParameterCodeSequenceAsync_Should_Return_Zero_WhenEmpty()
        {
            await ClearAllAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var max = await CreateRepo(ctx).GetMaxParameterCodeSequenceAsync();

            max.Should().Be(0);
        }

        [Fact]
        public async Task GetMaxParameterCodeSequenceAsync_Should_Return_Max_Existing_Sequence()
        {
            await ClearAllAsync();
            var (g, d, v) = await SeedDependenciesAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            await CreateRepo(ctx).CreateAsync(BuildEntity(g, d, v, code: "QP-000003", name: "Three"));
            await CreateRepo(ctx).CreateAsync(BuildEntity(g, d, v, code: "QP-000007", name: "Seven"));
            await CreateRepo(ctx).CreateAsync(BuildEntity(g, d, v, code: "QP-000005", name: "Five"));

            var max = await CreateRepo(ctx).GetMaxParameterCodeSequenceAsync();

            max.Should().Be(7);
        }
    }
}
