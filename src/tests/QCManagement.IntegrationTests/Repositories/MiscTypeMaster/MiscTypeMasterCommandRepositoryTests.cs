using Microsoft.EntityFrameworkCore;
using QCManagement.Infrastructure.Data;
using QCManagement.Infrastructure.Repositories.MiscTypeMaster;
using QCManagement.IntegrationTests.Common;
using static QCManagement.Domain.Common.BaseEntity;

namespace QCManagement.IntegrationTests.Repositories.MiscTypeMaster
{
    [Collection("DatabaseCollection")]
    public sealed class MiscTypeMasterCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public MiscTypeMasterCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private MiscTypeMasterCommandRepository CreateRepository(ApplicationDbContext ctx)
            => new MiscTypeMasterCommandRepository(ctx);

        private Domain.Entities.MiscTypeMaster BuildEntity(
            string code = "QP_GROUP",
            string description = "Quality Parameter Group") =>
            new Domain.Entities.MiscTypeMaster
            {
                MiscTypeCode = code,
                Description = description,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };

        private async Task ClearTableAsync() =>
            await _fixture.ClearTablesAsync("QC.QualitySpecificationParameter", "QC.QualitySpecification", "QC.QualityTemplateParameter", "QC.QualityTemplate", "QC.QualityParameter", "QC.MiscMaster", "QC.MiscTypeMaster");

        [Fact]
        public async Task CreateAsync_Should_Return_NewId_GreaterThanZero()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync();

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity());

            newId.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields_Correctly()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync();

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity("QP_GROUP", "Quality Parameter Group"));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.MiscTypeMaster.FirstOrDefaultAsync(x => x.Id == newId);

            saved.Should().NotBeNull();
            saved!.MiscTypeCode.Should().Be("QP_GROUP");
            saved.Description.Should().Be("Quality Parameter Group");
            saved.IsActive.Should().Be(Status.Active);
            saved.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public async Task CreateAsync_Should_Populate_Audit_Fields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync();

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity());
            ctx.ChangeTracker.Clear();

            var saved = await ctx.MiscTypeMaster.FirstOrDefaultAsync(x => x.Id == newId);

            saved!.CreatedBy.Should().Be(1);
            saved.CreatedByName.Should().Be("test-user");
            saved.CreatedIP.Should().Be("127.0.0.1");
            saved.CreatedDate.Should().NotBeNull();
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_UpdatedId()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync();
            var id = await CreateRepository(ctx).CreateAsync(BuildEntity("QP_GROUP", "Original"));
            ctx.ChangeTracker.Clear();

            var updatedId = await CreateRepository(ctx).UpdateAsync(new Domain.Entities.MiscTypeMaster
            {
                Id = id,
                Description = "Updated Description",
                IsActive = Status.Active
            });

            updatedId.Should().Be(id);
        }

        [Fact]
        public async Task UpdateAsync_Should_Persist_Mutable_Fields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync();
            var id = await CreateRepository(ctx).CreateAsync(BuildEntity("QP_GROUP", "Original"));
            ctx.ChangeTracker.Clear();

            await CreateRepository(ctx).UpdateAsync(new Domain.Entities.MiscTypeMaster
            {
                Id = id,
                Description = "Updated Description",
                IsActive = Status.Inactive
            });
            ctx.ChangeTracker.Clear();

            var saved = await ctx.MiscTypeMaster.FirstOrDefaultAsync(x => x.Id == id);
            saved!.Description.Should().Be("Updated Description");
            saved.IsActive.Should().Be(Status.Inactive);
        }

        [Fact]
        public async Task UpdateAsync_Should_Not_Change_Code()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync();
            var id = await CreateRepository(ctx).CreateAsync(BuildEntity("ORIG_CODE", "Original"));
            ctx.ChangeTracker.Clear();

            await CreateRepository(ctx).UpdateAsync(new Domain.Entities.MiscTypeMaster
            {
                Id = id,
                Description = "Updated Description",
                IsActive = Status.Active
            });
            ctx.ChangeTracker.Clear();

            var saved = await ctx.MiscTypeMaster.FirstOrDefaultAsync(x => x.Id == id);
            saved!.MiscTypeCode.Should().Be("ORIG_CODE");
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_Zero_WhenEntityNotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync();

            var result = await CreateRepository(ctx).UpdateAsync(new Domain.Entities.MiscTypeMaster
            {
                Id = 99999,
                Description = "Ghost",
                IsActive = Status.Active
            });

            result.Should().Be(0);
        }

        [Fact]
        public async Task UpdateAsync_Should_Populate_ModifiedAuditFields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync();
            var id = await CreateRepository(ctx).CreateAsync(BuildEntity());
            ctx.ChangeTracker.Clear();

            await CreateRepository(ctx).UpdateAsync(new Domain.Entities.MiscTypeMaster
            {
                Id = id,
                Description = "Modified",
                IsActive = Status.Active
            });
            ctx.ChangeTracker.Clear();

            var saved = await ctx.MiscTypeMaster.FirstOrDefaultAsync(x => x.Id == id);
            saved!.ModifiedBy.Should().Be(1);
            saved.ModifiedByName.Should().Be("test-user");
            saved.ModifiedIP.Should().Be("127.0.0.1");
            saved.ModifiedDate.Should().NotBeNull();
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_True_WhenEntityExists()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync();
            var id = await CreateRepository(ctx).CreateAsync(BuildEntity());
            ctx.ChangeTracker.Clear();

            var result = await CreateRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_SetIsDeleted_Flag()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync();
            var id = await CreateRepository(ctx).CreateAsync(BuildEntity());
            ctx.ChangeTracker.Clear();

            await CreateRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.MiscTypeMaster
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == id);

            saved!.IsDeleted.Should().Be(IsDelete.Deleted);
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_False_WhenEntityNotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync();

            var result = await CreateRepository(ctx).SoftDeleteAsync(99999, CancellationToken.None);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_False_WhenAlreadyDeleted()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync();
            var id = await CreateRepository(ctx).CreateAsync(BuildEntity());
            ctx.ChangeTracker.Clear();

            await CreateRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var result = await CreateRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);

            result.Should().BeFalse();
        }
    }
}
