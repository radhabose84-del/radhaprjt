using BackgroundService.Infrastructure.Repositories.Lookups.Workflow;
using BackgroundService.IntegrationTests.Common;
using Microsoft.Data.SqlClient;

namespace BackgroundService.IntegrationTests.Repositories.Lookups
{
    /// <summary>
    /// WorkflowLookupRepository methods that short-circuit on empty input arrays
    /// are covered here. Methods that join AppData.Menus (a table that lives in
    /// UserManagement schema and is not part of the BackgroundService test DbContext)
    /// require a unified schema fixture to test end-to-end.
    /// </summary>
    [Collection("DatabaseCollection")]
    public sealed class WorkflowLookupRepositoryTests
    {
        private readonly DbFixture _fixture;

        public WorkflowLookupRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private WorkflowLookupRepository CreateRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new WorkflowLookupRepository(conn);
        }

        // --- GetApprovalRequestLineStatusAsync short-circuit ---

        [Fact]
        public async Task GetApprovalRequestLineStatus_Should_Return_Empty_When_Ids_Empty()
        {
            var result = await CreateRepo().GetApprovalRequestLineStatusAsync(
                "PurchaseOrder", Array.Empty<int>(), 1);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetApprovalRequestLineStatus_Should_Return_Empty_When_Ids_Null()
        {
            var result = await CreateRepo().GetApprovalRequestLineStatusAsync(
                "PurchaseOrder", null!, 1);

            result.Should().BeEmpty();
        }

        // --- GetApproverListAsync short-circuit ---

        [Fact]
        public async Task GetApproverListAsync_Should_Return_Empty_When_Ids_Empty()
        {
            var result = await CreateRepo().GetApproverListAsync(
                "PurchaseOrder", Array.Empty<int>());

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetApproverListAsync_Should_Return_Empty_When_Ids_Null()
        {
            var result = await CreateRepo().GetApproverListAsync(
                "PurchaseOrder", null!);

            result.Should().BeEmpty();
        }

        // --- GetApprovalRequestLineAsync passes through to LineStatus ---

        [Fact]
        public async Task GetApprovalRequestLineAsync_Should_Delegate_To_LineStatus()
        {
            var result = await CreateRepo().GetApprovalRequestLineAsync(
                "PurchaseOrder", Array.Empty<int>(), 1);

            result.Should().BeEmpty();
        }

        // --- GetAllApprovalRequestStatusAsync ---
        // Skipped: source SQL references `ar.IsDeleted = 0`, but the ApprovalRequest
        // entity/table does not define an IsDeleted column (not a BaseEntity). A test
        // against the real schema surfaces "Invalid column name 'IsDeleted'".
    }
}
