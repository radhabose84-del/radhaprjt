using Contracts.Interfaces.Lookups.Users;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PurchaseManagement.Infrastructure.Data;
using PurchaseManagement.Infrastructure.Repositories.Quotation.RfqEntry;
using PurchaseManagement.IntegrationTests.Common;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.IntegrationTests.Repositories.Quotation.RfqEntry
{
    /// <summary>
    /// Integration tests for the RfqAttachment-related command repository methods.
    /// Uses raw SQL with FK constraints temporarily disabled to seed attachment rows
    /// without needing the full MiscMaster -> RfqStatus -> RfqMaster seeding chain.
    /// </summary>
    [Collection("DatabaseCollection")]
    public sealed class RfqAttachmentCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public RfqAttachmentCommandRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private RfqCommandRepository CreateRepo(ApplicationDbContext ctx)
        {
            var unitLookup = new Mock<IUnitLookup>(MockBehavior.Loose);
            return new RfqCommandRepository(ctx, _fixture.IpMock.Object, unitLookup.Object);
        }

        private async Task<int> SeedAttachmentDirectlyAsync(
            int rfqId,
            string filePath = "/tmp/test.pdf",
            int isDeleted = 0)
        {
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();

            await conn.ExecuteAsync(@"
                ALTER TABLE [Purchase].[RfqAttachment] NOCHECK CONSTRAINT ALL;
            ");

            return await conn.ExecuteScalarAsync<int>(@"
                INSERT INTO [Purchase].[RfqAttachment]
                    (RfqId, FileName, OriginalFileName, FilePath, FileType, FileSize,
                     IsActive, IsDeleted, CreatedBy, CreatedByName, CreatedIP, CreatedDate)
                OUTPUT INSERTED.Id
                VALUES (@RfqId, 'staged.pdf', 'original.pdf', @FilePath, 'application/pdf', 1024,
                        1, @IsDeleted, 1, 'test-user', '127.0.0.1', SYSDATETIMEOFFSET());",
                new { RfqId = rfqId, FilePath = filePath, IsDeleted = isDeleted });
        }

        [Fact]
        public async Task SoftDeleteAttachmentAsync_Should_Return_Null_When_NotFound()
        {
            await _fixture.ClearAllTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var result = await CreateRepo(ctx).SoftDeleteAttachmentAsync(1, 9999, CancellationToken.None);

            result.Should().BeNull();
        }

        [Fact]
        public async Task SoftDeleteAttachmentAsync_Should_Return_FilePath_When_Found()
        {
            await _fixture.ClearAllTablesAsync();
            var attachmentId = await SeedAttachmentDirectlyAsync(rfqId: 100, filePath: "/Resources/Purchase/RfqAttachments/a.pdf");

            await using var ctx = _fixture.CreateFreshDbContext();
            var result = await CreateRepo(ctx).SoftDeleteAttachmentAsync(100, attachmentId, CancellationToken.None);

            result.Should().Be("/Resources/Purchase/RfqAttachments/a.pdf");
        }

        [Fact]
        public async Task SoftDeleteAttachmentAsync_Should_Mark_IsDeleted_True()
        {
            await _fixture.ClearAllTablesAsync();
            var attachmentId = await SeedAttachmentDirectlyAsync(rfqId: 100);

            await using (var ctx = _fixture.CreateFreshDbContext())
            {
                await CreateRepo(ctx).SoftDeleteAttachmentAsync(100, attachmentId, CancellationToken.None);
            }

            await using var verifyCtx = _fixture.CreateFreshDbContext();
            var attachment = await verifyCtx.RfqAttachments
                .AsNoTracking()
                .FirstAsync(a => a.Id == attachmentId);

            attachment.IsDeleted.Should().Be(IsDelete.Deleted);
        }

        [Fact]
        public async Task SoftDeleteAttachmentAsync_Should_Return_Null_When_WrongRfqId()
        {
            await _fixture.ClearAllTablesAsync();
            var attachmentId = await SeedAttachmentDirectlyAsync(rfqId: 100);

            await using var ctx = _fixture.CreateFreshDbContext();
            var result = await CreateRepo(ctx).SoftDeleteAttachmentAsync(rfqId: 999, attachmentId, CancellationToken.None);

            result.Should().BeNull();
        }

        [Fact]
        public async Task SoftDeleteAttachmentAsync_Should_Return_Null_When_AlreadyDeleted()
        {
            await _fixture.ClearAllTablesAsync();
            var attachmentId = await SeedAttachmentDirectlyAsync(rfqId: 100, isDeleted: 1);

            await using var ctx = _fixture.CreateFreshDbContext();
            var result = await CreateRepo(ctx).SoftDeleteAttachmentAsync(100, attachmentId, CancellationToken.None);

            result.Should().BeNull();
        }

        [Fact]
        public async Task SoftDeleteAttachmentAsync_Should_Populate_ModifiedAudit_Fields()
        {
            await _fixture.ClearAllTablesAsync();
            var attachmentId = await SeedAttachmentDirectlyAsync(rfqId: 100);

            await using (var ctx = _fixture.CreateFreshDbContext())
            {
                await CreateRepo(ctx).SoftDeleteAttachmentAsync(100, attachmentId, CancellationToken.None);
            }

            await using var verifyCtx = _fixture.CreateFreshDbContext();
            var attachment = await verifyCtx.RfqAttachments
                .AsNoTracking()
                .FirstAsync(a => a.Id == attachmentId);

            attachment.ModifiedBy.Should().Be(1);
            attachment.ModifiedByName.Should().Be("test-user");
            attachment.ModifiedIP.Should().Be("127.0.0.1");
            attachment.ModifiedDate.Should().NotBeNull();
        }
    }
}
