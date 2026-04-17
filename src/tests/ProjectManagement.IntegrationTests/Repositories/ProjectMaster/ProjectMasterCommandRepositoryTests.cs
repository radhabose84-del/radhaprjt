using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ProjectManagement.Application.Common.Interfaces.IMiscMaster;
using ProjectManagement.Domain.Common;
using ProjectManagement.Infrastructure.Repositories.ProjectMaster;

namespace ProjectManagement.IntegrationTests.Repositories.ProjectMaster
{
    [Collection("DatabaseCollection")]
    public sealed class ProjectMasterCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public ProjectMasterCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private ProjectMasterCommandRepository CreateRepository(
            ProjectManagement.Infrastructure.Data.ApplicationDbContext ctx)
        {
            var miscMock = new Mock<IMiscMasterQueryRepository>(MockBehavior.Loose);
            miscMock
                .Setup(m => m.GetMiscMasterByName(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new ProjectManagement.Domain.Entities.MiscMaster
                {
                    Id = 1,
                    Code = "PENDING",
                    Description = "Pending"
                });

            var mapperMock = new Mock<IMapper>(MockBehavior.Loose);
            return new ProjectMasterCommandRepository(ctx, miscMock.Object, mapperMock.Object);
        }

        private static ProjectManagement.Domain.Entities.ProjectMaster BuildEntity(
            string name = "Test Project") =>
            new ProjectManagement.Domain.Entities.ProjectMaster
            {
                ProjectCode = "WILL_BE_GENERATED",
                ProjectName = name,
                ProjectTypeId = 1,
                UnitId = 1,
                DepartmentId = 1,
                BudgetAmount = 100000m,
                BudgetYearId = 1,
                CostCenterId = 1,
                CurrencyId = 1,
                StartDate = DateTimeOffset.UtcNow,
                EndDate = DateTimeOffset.UtcNow.AddMonths(6),
                ProjectCategoryId = 1,
                AssetGroupId = 1,
                StatusId = 1,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            };

        private async Task ClearTablesAsync(ProjectManagement.Infrastructure.Data.ApplicationDbContext ctx) =>
            await _fixture.ClearAllTablesAsync();

        // --- CREATE ---

        [Fact]
        public async Task CreateAsync_Should_Return_Entity_With_Id_GreaterThanZero()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var result = await CreateRepository(ctx).CreateAsync(BuildEntity());

            result.Id.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields_Correctly()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var result = await CreateRepository(ctx).CreateAsync(BuildEntity("My Project"));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.ProjectMaster.FirstOrDefaultAsync(x => x.Id == result.Id);

            saved.Should().NotBeNull();
            saved!.ProjectName.Should().Be("My Project");
            saved.IsActive.Should().Be(BaseEntity.Status.Active);
            saved.IsDeleted.Should().Be(BaseEntity.IsDelete.NotDeleted);
        }

        [Fact]
        public async Task CreateAsync_Should_AutoGenerate_ProjectCode()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var result = await CreateRepository(ctx).CreateAsync(BuildEntity());

            result.ProjectCode.Should().StartWith("PROJ-");
        }

        [Fact]
        public async Task CreateAsync_Should_Populate_Audit_Fields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var result = await CreateRepository(ctx).CreateAsync(BuildEntity());
            ctx.ChangeTracker.Clear();

            var saved = await ctx.ProjectMaster.FirstOrDefaultAsync(x => x.Id == result.Id);

            saved!.CreatedBy.Should().Be(1);
            saved.CreatedByName.Should().Be("test-user");
            saved.CreatedDate.Should().NotBeNull();
        }

        // --- UPDATE ---

        [Fact]
        public async Task UpdateAsync_Should_Persist_Changes()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var created = await CreateRepository(ctx).CreateAsync(BuildEntity("Original Name"));
            ctx.ChangeTracker.Clear();

            var toUpdate = await ctx.ProjectMaster.FirstAsync(x => x.Id == created.Id);
            toUpdate.ProjectName = "Updated Name";

            await CreateRepository(ctx).UpdateAsync(toUpdate);
            ctx.ChangeTracker.Clear();

            var updated = await ctx.ProjectMaster.FirstOrDefaultAsync(x => x.Id == created.Id);
            updated!.ProjectName.Should().Be("Updated Name");
        }

        // --- SOFT DELETE ---

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_True_When_Successful()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var created = await CreateRepository(ctx).CreateAsync(BuildEntity());
            ctx.ChangeTracker.Clear();

            var result = await CreateRepository(ctx).SoftDeleteAsync(created.Id);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Set_IsDeleted_Flag()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var created = await CreateRepository(ctx).CreateAsync(BuildEntity());
            ctx.ChangeTracker.Clear();

            await CreateRepository(ctx).SoftDeleteAsync(created.Id);
            ctx.ChangeTracker.Clear();

            var deleted = await ctx.ProjectMaster
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == created.Id);

            deleted!.IsDeleted.Should().Be(BaseEntity.IsDelete.Deleted);
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_False_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var result = await CreateRepository(ctx).SoftDeleteAsync(9999);

            result.Should().BeFalse();
        }

        // --- GET BY ID ---

        [Fact]
        public async Task GetByIdAsync_Should_Return_Entity()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var created = await CreateRepository(ctx).CreateAsync(BuildEntity("Named Project"));
            ctx.ChangeTracker.Clear();

            var result = await CreateRepository(ctx).GetByIdAsync(created.Id);

            result.Should().NotBeNull();
            result!.ProjectName.Should().Be("Named Project");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var result = await CreateRepository(ctx).GetByIdAsync(9999);

            result.Should().BeNull();
        }
    }
}
