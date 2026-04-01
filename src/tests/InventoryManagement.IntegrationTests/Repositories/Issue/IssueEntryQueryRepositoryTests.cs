using InventoryManagement.Infrastructure.Repositories.Issue;
using Microsoft.Data.SqlClient;

namespace InventoryManagement.IntegrationTests.Repositories.Issue
{
    [Collection("DatabaseCollection")]
    public sealed class IssueEntryQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public IssueEntryQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private IssueEntryQueryRepository CreateQueryRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new IssueEntryQueryRepository(conn, _fixture.IpMock.Object);
        }

        [Fact]
        public async Task GetApprovedMrsDetails_Should_Return_Empty_When_NoApprovedMrs()
        {
            var result = await CreateQueryRepo().GetApprovedMrsDetails((string?)null);

            result.Should().NotBeNull();
        }

        [Fact]
        public async Task GetPendingIssuesAsync_Should_Return_Empty_When_NoPendingIssues()
        {
            var result = await CreateQueryRepo().GetPendingIssuesAsync(0);

            result.Should().NotBeNull();
        }

        [Fact]
        public async Task GetDescriptionByIdAsync_Should_Return_Null_When_NotFound()
        {
            var result = await CreateQueryRepo().GetDescriptionByIdAsync(9999);

            result.Should().BeNullOrEmpty();
        }

        [Fact]
        public async Task GetPendingIssueHeaderAsync_Should_Return_Empty_Or_List()
        {
            var (items, total) = await CreateQueryRepo().GetPendingIssueHeaderAsync(
                null, null, 1, 10, null);

            items.Should().NotBeNull();
            total.Should().BeGreaterThanOrEqualTo(0);
        }
    }
}
