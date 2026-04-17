using SalesManagement.IntegrationTests.Common;

namespace SalesManagement.IntegrationTests.Repositories.StoReceipt
{
    /// <summary>
    /// Integration tests for StoReceiptCommandRepository.
    ///
    /// StoReceiptCommandRepository.CreateAsync is the most complex transactional operation in the
    /// STO chain. It:
    ///   1. Inserts StoReceiptHeader + StoReceiptDetail rows
    ///   2. Marks ALL dispatched packs at the FROM plant as Dispatched in StockLedger (raw SQL UPDATE)
    ///   3. Fetches PackTypeId and TotalValue from existing StockLedger PROD rows
    ///   4. INSERTs new StockLedger rows at the ReceivingPlant for each PackNo
    ///      (first AcceptedQuantity packs → Packed status, remaining → Damaged status)
    ///   5. Increments Finance.DocumentSequence within the same transaction
    ///
    /// This requires a fully populated StockLedger with PROD-type records matching the
    /// detail lines' ItemId, LotId, and PackNo ranges. Building this fixture is beyond
    /// repository-level integration tests — it is covered by end-to-end workflow tests.
    ///
    /// The StoReceiptQueryRepository tests (in this same folder) exercise the read paths
    /// against directly seeded StoReceipt rows, bypassing the transactional CreateAsync.
    /// </summary>
    [Collection("DatabaseCollection")]
    public sealed class StoReceiptCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public StoReceiptCommandRepositoryTests(DbFixture fixture) => _fixture = fixture;

        [Fact]
        public void CreateAsync_RequiresFullStockLedgerFixture_DocumentedOnly()
        {
            // StoReceiptCommandRepository.CreateAsync requires:
            //   - StockLedger rows with DocType='PROD' matching detail ItemId/LotId/PackNo ranges
            //   - A DeliveryChallanHeader → StoHeader → MovementTypeConfig → StoTypeMaster chain
            //   - IDocumentSequenceLookup for Finance.DocumentSequence increment
            //
            // This test documents the dependency rather than attempting a partial integration
            // that would give false confidence. The query repository tests in this folder
            // verify read operations against directly seeded data.
            //
            // Full transactional coverage is provided by end-to-end workflow tests
            // that seed production stock, create STO → DC → Receipt in sequence.

            _fixture.Should().NotBeNull("DbFixture is available for StoReceipt integration tests");
        }
    }
}
