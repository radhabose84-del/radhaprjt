using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PurchaseManagement.Infrastructure.Repositories.PurchaseOrder;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.IntegrationTests.Repositories.PurchaseOrder
{
    /// <summary>
    /// Real data-flow tests for the now-fixed PODocumentQueryRepository.
    ///
    /// Coverage:
    ///   GetDocumentDirectoryAsync — Dapper SELECT from Purchase.MiscTypeMaster
    ///   GetBaseDirectoryAsync     — Dapper SELECT from Budget.MiscTypeMaster (cross-schema → SqlException)
    ///   DeleteFileDetailsDocumentAsync — EF Core DELETE on Purchase.PurchaseDocuments
    ///   GetPODocumentIdsAsync     — EF Core SELECT on Purchase.PurchaseDocuments
    /// </summary>
    [Collection("DatabaseCollection")]
    public sealed class PODocumentQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public PODocumentQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private PODocumentQueryRepository CreateRepo(PurchaseManagement.Infrastructure.Data.ApplicationDbContext ctx) =>
            new(new SqlConnection(_fixture.ConnectionString), ctx);

        private async Task SeedDocumentPathMiscTypeAsync(
            string description,
            Status active = Status.Active,
            IsDelete deleted = IsDelete.NotDeleted)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var miscType = new PurchaseManagement.Domain.Entities.MiscTypeMaster
            {
                MiscTypeCode = "PoDocument",   // == MiscEnumEntity.DocumentPath
                Description = description,
                IsActive = active,
                IsDeleted = deleted
            };
            ctx.MiscTypeMaster.Add(miscType);
            await ctx.SaveChangesAsync();
        }

        /// <summary>
        /// Inserts a PurchaseDocument row directly via raw SQL with FK constraints temporarily
        /// disabled — avoids having to seed PurchaseOrderHeader and MiscMaster parent rows
        /// (which carry their own deep FK graphs in this module).
        /// </summary>
        private async Task SeedPurchaseDocumentRawAsync(int id, int poId, int documentId, string fileName)
        {
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();

            await conn.ExecuteAsync(@"
                ALTER TABLE Purchase.PurchaseDocuments NOCHECK CONSTRAINT ALL;

                SET IDENTITY_INSERT Purchase.PurchaseDocuments ON;
                INSERT INTO Purchase.PurchaseDocuments (Id, PoId, DocumentId, FileName, UploadedDate)
                VALUES (@Id, @PoId, @DocumentId, @FileName, SYSDATETIMEOFFSET());
                SET IDENTITY_INSERT Purchase.PurchaseDocuments OFF;

                ALTER TABLE Purchase.PurchaseDocuments WITH NOCHECK CHECK CONSTRAINT ALL;",
                new { Id = id, PoId = poId, DocumentId = documentId, FileName = fileName });
        }

        // ── GetDocumentDirectoryAsync ─────────────────────────────────────────

        [Fact]
        public async Task GetDocumentDirectoryAsync_Should_Return_Empty_When_NoMatch()
        {
            await _fixture.ClearAllTablesAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            var result = await CreateRepo(ctx).GetDocumentDirectoryAsync();

            result.Should().Be(string.Empty);
        }

        [Fact]
        public async Task GetDocumentDirectoryAsync_Should_Return_Description_When_Active_Match_Exists()
        {
            await _fixture.ClearAllTablesAsync();
            await SeedDocumentPathMiscTypeAsync(description: "/srv/po-documents");

            await using var ctx = _fixture.CreateFreshDbContext();
            var result = await CreateRepo(ctx).GetDocumentDirectoryAsync();

            result.Should().Be("/srv/po-documents");
        }

        [Fact]
        public async Task GetDocumentDirectoryAsync_Should_Exclude_Inactive()
        {
            await _fixture.ClearAllTablesAsync();
            await SeedDocumentPathMiscTypeAsync("/inactive", active: Status.Inactive);

            await using var ctx = _fixture.CreateFreshDbContext();
            var result = await CreateRepo(ctx).GetDocumentDirectoryAsync();

            result.Should().Be(string.Empty);
        }

        [Fact]
        public async Task GetDocumentDirectoryAsync_Should_Exclude_SoftDeleted()
        {
            await _fixture.ClearAllTablesAsync();
            await SeedDocumentPathMiscTypeAsync("/deleted", deleted: IsDelete.Deleted);

            await using var ctx = _fixture.CreateFreshDbContext();
            var result = await CreateRepo(ctx).GetDocumentDirectoryAsync();

            result.Should().Be(string.Empty);
        }

        [Fact]
        public async Task GetDocumentDirectoryAsync_Should_Return_Latest_When_Multiple_Active()
        {
            await _fixture.ClearAllTablesAsync();
            await SeedDocumentPathMiscTypeAsync("/old");
            await SeedDocumentPathMiscTypeAsync("/newer");
            await SeedDocumentPathMiscTypeAsync("/latest");

            await using var ctx = _fixture.CreateFreshDbContext();
            var result = await CreateRepo(ctx).GetDocumentDirectoryAsync();

            // Production query: ORDER BY Id DESC LIMIT 1 → highest Id wins
            result.Should().Be("/latest");
        }

        // ── GetBaseDirectoryAsync ─────────────────────────────────────────────

        [Fact]
        public async Task GetBaseDirectoryAsync_Should_Throw_SqlException_Because_Budget_Schema_Missing()
        {
            // GetBaseDirectoryAsync queries Budget.MiscTypeMaster which lives in
            // BudgetManagement's schema — not provisioned in the Purchase test DB
            // (per CLAUDE.md cross-module isolation). Verifying the expected failure mode.
            await using var ctx = _fixture.CreateFreshDbContext();

            Func<Task> act = async () => await CreateRepo(ctx).GetBaseDirectoryAsync();

            await act.Should().ThrowAsync<SqlException>();
        }

        // ── DeleteFileDetailsDocumentAsync ────────────────────────────────────

        [Fact]
        public async Task DeleteFileDetailsDocumentAsync_Should_Return_False_When_NotFound()
        {
            await _fixture.ClearAllTablesAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            var result = await CreateRepo(ctx).DeleteFileDetailsDocumentAsync(999, 999, "missing.pdf");

            result.Should().BeFalse();
        }

        [Fact]
        public async Task DeleteFileDetailsDocumentAsync_Should_Return_True_And_Remove_Row_When_Match_Exists()
        {
            await _fixture.ClearAllTablesAsync();
            await SeedPurchaseDocumentRawAsync(id: 1, poId: 100, documentId: 50, fileName: "invoice.pdf");

            await using var ctx = _fixture.CreateFreshDbContext();
            var result = await CreateRepo(ctx).DeleteFileDetailsDocumentAsync(1, 100, "invoice.pdf");

            result.Should().BeTrue();

            await using var verifyCtx = _fixture.CreateFreshDbContext();
            var remaining = await verifyCtx.PurchaseDocuments.AnyAsync(x => x.Id == 1);
            remaining.Should().BeFalse();
        }

        [Fact]
        public async Task DeleteFileDetailsDocumentAsync_Should_Return_False_When_FileName_Mismatch()
        {
            await _fixture.ClearAllTablesAsync();
            await SeedPurchaseDocumentRawAsync(id: 2, poId: 200, documentId: 50, fileName: "real.pdf");

            await using var ctx = _fixture.CreateFreshDbContext();
            var result = await CreateRepo(ctx).DeleteFileDetailsDocumentAsync(2, 200, "wrong-name.pdf");

            result.Should().BeFalse();

            await using var verifyCtx = _fixture.CreateFreshDbContext();
            var stillThere = await verifyCtx.PurchaseDocuments.AnyAsync(x => x.Id == 2);
            stillThere.Should().BeTrue();
        }

        // ── GetPODocumentIdsAsync ─────────────────────────────────────────────

        [Fact]
        public async Task GetPODocumentIdsAsync_With_Zero_PoId_Returns_Empty_Via_Guard()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var result = await CreateRepo(ctx).GetPODocumentIdsAsync(0);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetPODocumentIdsAsync_With_Negative_PoId_Returns_Empty_Via_Guard()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var result = await CreateRepo(ctx).GetPODocumentIdsAsync(-5);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetPODocumentIdsAsync_With_NoData_Returns_Empty()
        {
            await _fixture.ClearAllTablesAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            var result = await CreateRepo(ctx).GetPODocumentIdsAsync(42);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetPODocumentIdsAsync_Returns_DocumentIds_Filtered_By_PoId()
        {
            // The schema enforces UNIQUE (PoId, DocumentId) so the .Distinct() in
            // production is defensively redundant but harmless. We verify the PoId
            // filter separates documents across orders.
            await _fixture.ClearAllTablesAsync();
            await SeedPurchaseDocumentRawAsync(id: 10, poId: 100, documentId: 1, fileName: "a.pdf");
            await SeedPurchaseDocumentRawAsync(id: 11, poId: 100, documentId: 2, fileName: "b.pdf");
            await SeedPurchaseDocumentRawAsync(id: 12, poId: 100, documentId: 3, fileName: "c.pdf");
            // Different PO — must not leak into result
            await SeedPurchaseDocumentRawAsync(id: 20, poId: 999, documentId: 4, fileName: "z.pdf");

            await using var ctx = _fixture.CreateFreshDbContext();
            var result = await CreateRepo(ctx).GetPODocumentIdsAsync(100);

            result.Should().BeEquivalentTo(new[] { 1, 2, 3 });
        }

        [Fact]
        public async Task GetPODocumentIdsAsync_Excludes_Rows_With_NonPositive_DocumentId()
        {
            await _fixture.ClearAllTablesAsync();
            await SeedPurchaseDocumentRawAsync(id: 30, poId: 200, documentId: 5, fileName: "ok.pdf");
            await SeedPurchaseDocumentRawAsync(id: 31, poId: 200, documentId: 0, fileName: "zero.pdf");

            await using var ctx = _fixture.CreateFreshDbContext();
            var result = await CreateRepo(ctx).GetPODocumentIdsAsync(200);

            result.Should().BeEquivalentTo(new[] { 5 });
        }
    }
}
