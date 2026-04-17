using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ProjectManagement.Domain.Common;
using ProjectManagement.Infrastructure.Data;
using ProjectManagement.Infrastructure.Repositories;
using ProjectManagement.IntegrationTests.Common;
using static ProjectManagement.Domain.Common.BaseEntity;

namespace ProjectManagement.IntegrationTests.Repositories.Document
{
    [Collection("DatabaseCollection")]
    public sealed class DocumentQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public DocumentQueryRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private DocumentQueryRepository CreateRepo(ApplicationDbContext ctx) =>
            new(ctx, new SqlConnection(_fixture.ConnectionString));

        // ProjectDocument requires a ProjectMaster FK; seed minimal one.
        private async Task<int> EnsureProjectAsync(string code = "DOC_PROJ")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var existing = await ctx.ProjectMaster.FirstOrDefaultAsync(p => p.ProjectCode == code);
            if (existing != null) return existing.Id;
            var p = new ProjectManagement.Domain.Entities.ProjectMaster
            {
                ProjectCode = code,
                ProjectName = "Doc Project",
                ProjectTypeId = 1,
                UnitId = 1,
                DepartmentId = 1,
                BudgetAmount = 0m,
                BudgetYearId = 1,
                CostCenterId = 1,
                CurrencyId = 1,
                StartDate = DateTimeOffset.UtcNow,
                EndDate = DateTimeOffset.UtcNow.AddMonths(1),
                ProjectCategoryId = 1,
                AssetGroupId = 1,
                StatusId = 1,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
            await ctx.ProjectMaster.AddAsync(p);
            await ctx.SaveChangesAsync();
            return p.Id;
        }

        private async Task EnsureMiscTypeAsync(string typeCode, string description)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var existing = await ctx.MiscTypeMaster.FirstOrDefaultAsync(t => t.MiscTypeCode == typeCode);
            if (existing != null)
            {
                if (existing.Description != description)
                {
                    existing.Description = description;
                    await ctx.SaveChangesAsync();
                }
                return;
            }
            await ctx.MiscTypeMaster.AddAsync(new ProjectManagement.Domain.Entities.MiscTypeMaster
            {
                MiscTypeCode = typeCode,
                Description = description,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            });
            await ctx.SaveChangesAsync();
        }

        private async Task<int> SeedDocumentAsync(int projectId, string fileName, int documentId = 1)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var d = new ProjectManagement.Domain.Entities.ProjectDocument
            {
                ProjectId = projectId,
                DocumentId = documentId,
                FileName = fileName,
                UploadedDate = DateTimeOffset.UtcNow
            };
            await ctx.ProjectDocument.AddAsync(d);
            await ctx.SaveChangesAsync();
            return d.Id;
        }

        private async Task ClearAsync() =>
            await _fixture.ClearAllTablesAsync();

        // --- GetDocumentDirectoryAsync ---

        [Fact]
        public async Task GetDocumentDirectoryAsync_Should_Return_Description_Of_DocumentPath_MiscType()
        {
            await EnsureMiscTypeAsync(MiscEnumEntity.DocumentPath, "/uploads/projects");
            await using var ctx = _fixture.CreateFreshDbContext();

            var result = await CreateRepo(ctx).GetDocumentDirectoryAsync();

            result.Should().Be("/uploads/projects");
        }

        [Fact]
        public async Task GetDocumentDirectoryAsync_Should_Return_Null_When_MiscType_Missing()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            // Soft-delete the DocumentPath misc type so the query returns no rows.
            await ctx.Database.ExecuteSqlRawAsync(
                "UPDATE [Project].[MiscTypeMaster] SET IsDeleted = 1 WHERE MiscTypeCode = @p0",
                MiscEnumEntity.DocumentPath);

            var result = await CreateRepo(ctx).GetDocumentDirectoryAsync();

            result.Should().BeNull();

            // Restore for other tests
            await ctx.Database.ExecuteSqlRawAsync(
                "UPDATE [Project].[MiscTypeMaster] SET IsDeleted = 0 WHERE MiscTypeCode = @p0",
                MiscEnumEntity.DocumentPath);
        }

        // --- GetBaseDirectoryAsync ---

        [Fact]
        public async Task GetBaseDirectoryAsync_Should_Throw_NotSupportedException()
        {
            // Documents the existing bug in DocumentQueryRepository.GetBaseDirectoryAsync:
            // the SQL parameter @Code is bound to an anonymous object (`new { poDocument = ... }`),
            // not a string, so Dapper throws NotSupportedException at execution time.
            // This test pins the current behavior; if/when the bug is fixed, update this assertion.
            await using var ctx = _fixture.CreateFreshDbContext();

            Func<Task> act = async () => await CreateRepo(ctx).GetBaseDirectoryAsync();

            await act.Should().ThrowAsync<NotSupportedException>()
                .WithMessage("*cannot be used as a parameter value*");
        }

        // --- DeleteFileDetailsDocumentAsync ---

        [Fact]
        public async Task DeleteFileDetailsDocumentAsync_Should_Return_True_When_Removed()
        {
            await ClearAsync();
            var projectId = await EnsureProjectAsync();
            var id = await SeedDocumentAsync(projectId, "f1.pdf");
            await using var ctx = _fixture.CreateFreshDbContext();

            var result = await CreateRepo(ctx).DeleteFileDetailsDocumentAsync(id, projectId, "f1.pdf");

            result.Should().BeTrue();
            (await ctx.ProjectDocument.AnyAsync(d => d.Id == id)).Should().BeFalse();
        }

        [Fact]
        public async Task DeleteFileDetailsDocumentAsync_Should_Return_False_When_FileName_Mismatch()
        {
            await ClearAsync();
            var projectId = await EnsureProjectAsync();
            var id = await SeedDocumentAsync(projectId, "f2.pdf");
            await using var ctx = _fixture.CreateFreshDbContext();

            var result = await CreateRepo(ctx).DeleteFileDetailsDocumentAsync(id, projectId, "wrong.pdf");

            result.Should().BeFalse();
            (await ctx.ProjectDocument.AnyAsync(d => d.Id == id)).Should().BeTrue();
        }

        [Fact]
        public async Task DeleteFileDetailsDocumentAsync_Should_Return_False_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            var result = await CreateRepo(ctx).DeleteFileDetailsDocumentAsync(9999999, 1, "x.pdf");

            result.Should().BeFalse();
        }

        // --- GetUploadDocumentIdsAsync ---

        [Fact]
        public async Task GetUploadDocumentIdsAsync_Should_Return_Distinct_DocumentIds()
        {
            await ClearAsync();
            var projectId = await EnsureProjectAsync();
            await SeedDocumentAsync(projectId, "a.pdf", documentId: 5);
            await SeedDocumentAsync(projectId, "b.pdf", documentId: 5); // duplicate documentId
            await SeedDocumentAsync(projectId, "c.pdf", documentId: 7);
            await using var ctx = _fixture.CreateFreshDbContext();

            var result = await CreateRepo(ctx).GetUploadDocumentIdsAsync(projectId);

            result.Should().HaveCount(2);
            result.Should().Contain(new[] { 5, 7 });
        }

        [Fact]
        public async Task GetUploadDocumentIdsAsync_Should_Return_Empty_When_Negative_Id()
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            var result = await CreateRepo(ctx).GetUploadDocumentIdsAsync(-1);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetUploadDocumentIdsAsync_Should_Return_Empty_When_Zero_Id()
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            var result = await CreateRepo(ctx).GetUploadDocumentIdsAsync(0);

            result.Should().BeEmpty();
        }
    }
}
