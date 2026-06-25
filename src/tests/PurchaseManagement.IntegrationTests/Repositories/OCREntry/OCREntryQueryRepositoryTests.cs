using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Party;
using Contracts.Interfaces.Lookups.Production;
using Contracts.Interfaces.Lookups.QC;
using Contracts.Interfaces.Lookups.Users;
using Microsoft.Data.SqlClient;
using PurchaseManagement.Infrastructure.Repositories.OCREntry;
using PurchaseManagement.IntegrationTests.Common;

namespace PurchaseManagement.IntegrationTests.Repositories.OCREntry
{
    [Collection("DatabaseCollection")]
    public sealed class OCREntryQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public OCREntryQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private OCREntryQueryRepository CreateQueryRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new OCREntryQueryRepository(
                conn,
                new Mock<ISupplierLookup>(MockBehavior.Loose).Object,
                new Mock<ILocationMasterLookup>(MockBehavior.Loose).Object,
                new Mock<IStationLookup>(MockBehavior.Loose).Object,
                new Mock<IItemLookup>(MockBehavior.Loose).Object,
                new Mock<ICountMasterLookup>(MockBehavior.Loose).Object,
                new Mock<IPackTypeLookup>(MockBehavior.Loose).Object,
                new Mock<IUOMLookup>(MockBehavior.Loose).Object,
                new Mock<IQualityTemplateLookup>(MockBehavior.Loose).Object);
        }

        // --- GetFreightForOcrAsync ---
        // Executes the real OCR -> RawMaterialPOHeader -> FreightRfqHeader join against the schema.
        // A non-existent OCR yields (null, null); a bad column/table name would surface here.

        [Fact]
        public async Task GetFreightForOcrAsync_Should_Return_Nulls_When_No_Freight()
        {
            await _fixture.ClearAllTablesAsync();

            var (perBale, total) = await CreateQueryRepo().GetFreightForOcrAsync(999999);

            perBale.Should().BeNull();
            total.Should().BeNull();
        }
    }
}
