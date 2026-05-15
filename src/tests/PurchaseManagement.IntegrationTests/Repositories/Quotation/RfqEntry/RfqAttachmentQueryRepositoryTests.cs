using Dapper;
using Microsoft.Data.SqlClient;
using PurchaseManagement.Application.Common.Interfaces.IMiscMaster;
using PurchaseManagement.Infrastructure.Data;
using PurchaseManagement.Infrastructure.Repositories.Quotation.RfqEntry;
using PurchaseManagement.IntegrationTests.Common;

namespace PurchaseManagement.IntegrationTests.Repositories.Quotation.RfqEntry
{
    /// <summary>
    /// Integration tests for the RfqAttachment-related query repository methods.
    /// </summary>
    [Collection("DatabaseCollection")]
    public sealed class RfqAttachmentQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public RfqAttachmentQueryRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private RfqQueryRepository CreateRepo(ApplicationDbContext ctx)
        {
            var miscMock = new Mock<IMiscMasterQueryRepository>(MockBehavior.Loose);
            return new RfqQueryRepository(ctx, _fixture.IpMock.Object, miscMock.Object);
        }

        private async Task<int> SeedAttachmentDirectlyAsync(int rfqId, int isDeleted = 0)
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
                VALUES (@RfqId, 'staged.pdf', 'original.pdf', '/tmp/x.pdf', 'application/pdf', 1024,
                        1, @IsDeleted, 1, 'test-user', '127.0.0.1', SYSDATETIMEOFFSET());",
                new { RfqId = rfqId, IsDeleted = isDeleted });
        }

        [Fact]
        public async Task AttachmentExistsAsync_Should_Return_False_When_NotFound()
        {
            await _fixture.ClearAllTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var result = await CreateRepo(ctx).AttachmentExistsAsync(1, 9999, CancellationToken.None);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task AttachmentExistsAsync_Should_Return_True_When_Exists_And_Not_Deleted()
        {
            await _fixture.ClearAllTablesAsync();
            var attachmentId = await SeedAttachmentDirectlyAsync(rfqId: 100);

            await using var ctx = _fixture.CreateFreshDbContext();
            var result = await CreateRepo(ctx).AttachmentExistsAsync(100, attachmentId, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task AttachmentExistsAsync_Should_Return_False_When_SoftDeleted()
        {
            await _fixture.ClearAllTablesAsync();
            var attachmentId = await SeedAttachmentDirectlyAsync(rfqId: 100, isDeleted: 1);

            await using var ctx = _fixture.CreateFreshDbContext();
            var result = await CreateRepo(ctx).AttachmentExistsAsync(100, attachmentId, CancellationToken.None);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task AttachmentExistsAsync_Should_Return_False_When_WrongRfqId()
        {
            await _fixture.ClearAllTablesAsync();
            var attachmentId = await SeedAttachmentDirectlyAsync(rfqId: 100);

            await using var ctx = _fixture.CreateFreshDbContext();
            var result = await CreateRepo(ctx).AttachmentExistsAsync(rfqId: 999, attachmentId, CancellationToken.None);

            result.Should().BeFalse();
        }
    }
}
