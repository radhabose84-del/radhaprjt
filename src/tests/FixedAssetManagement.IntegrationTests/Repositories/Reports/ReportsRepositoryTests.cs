using Contracts.Interfaces;
using FAM.Application.Common.Interfaces;
using FAM.Infrastructure.Repositories.Reports;
using Microsoft.Data.SqlClient;

namespace FixedAssetManagement.IntegrationTests.Repositories.Reports
{
    // =====================================================================================
    // BLOCKED - ReportsRepository depends on database objects that are not created by EF.
    //
    //  * AssetAuditReportAsync      -> requires SQL table-valued function FixedAsset.fn_GetAuditComparison
    //  * AssetReportAsync           -> requires stored procedure              dbo.Rpt_AssetReport
    //  * AssetTransferReportAsync   -> requires SQL view                      dbo.vw_AssetTransferStatus
    //
    // DbFixture creates the schema from ApplicationDbContext entities via EnsureCreatedAsync,
    // which only produces tables - it does not create functions, stored procedures, or views.
    // Running any of these methods against the fresh test DB will fail with "Invalid object name"
    // or equivalent SQL errors.
    //
    // Two options to enable these tests later:
    //   (1) Extend DbFixture to CREATE the function, procedure, and view (and seed realistic
    //       rows across the multiple underlying tables each one reads from), or
    //   (2) Run these integration tests against a DB snapshot that already contains the
    //       analytics objects.
    // Either option requires non-trivial setup beyond the current scope.
    // =====================================================================================
    [Collection("DatabaseCollection")]
    public sealed class ReportsRepositoryTests
    {
        private const string BlockedReason =
            "BLOCKED - ReportsRepository queries a SQL function, stored procedure, and view that are " +
            "not created by EnsureCreatedAsync. See class XML doc for the two options to enable.";

        private readonly DbFixture _fixture;

        public ReportsRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private ReportsRepository CreateRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            var ipMock = new Mock<IIPAddressService>(MockBehavior.Loose);
            ipMock.Setup(x => x.GetUnitId()).Returns(1);
            ipMock.Setup(x => x.GetCompanyId()).Returns(1);
            return new ReportsRepository(conn, ipMock.Object);
        }

        [Fact]
        public void Constructor_Should_Accept_Connection_And_IpAddressService()
        {
            var sut = CreateRepo();

            sut.Should().NotBeNull();
        }

        [Fact(Skip = BlockedReason)]
        public async Task AssetAuditReportAsync_Should_Return_Rows_From_Function()
        {
            var result = await CreateRepo().AssetAuditReportAsync(auditTypeId: 1);

            result.Should().NotBeNull();
        }

        [Fact(Skip = BlockedReason)]
        public async Task AssetReportAsync_Should_Return_Rows_From_StoredProcedure()
        {
            var result = await CreateRepo().AssetReportAsync(
                DateTimeOffset.UtcNow.AddYears(-1),
                DateTimeOffset.UtcNow);

            result.Should().NotBeNull();
        }

        [Fact(Skip = BlockedReason)]
        public async Task AssetTransferReportAsync_Should_Return_Rows_From_View()
        {
            var result = await CreateRepo().AssetTransferReportAsync(
                DateTimeOffset.UtcNow.AddYears(-1),
                DateTimeOffset.UtcNow);

            result.Should().NotBeNull();
        }
    }
}
