using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Finance;
using Contracts.Interfaces.Lookups.Users;
using Microsoft.EntityFrameworkCore;
using PurchaseManagement.Infrastructure.Data;
using PurchaseManagement.Infrastructure.Repositories.Quotation.RfqEntry;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.IntegrationTests.Repositories.Quotation.RfqEntry
{
    /// <summary>
    /// Integration tests for RfqCommandRepository.
    ///
    /// COMPLEXITY NOTE:
    /// RFQ (Request for Quotation) is the entry point for the quotation workflow:
    /// - RfqMaster contains: RfqCode, RfqDate, InitiationTypeId, RfqStatusId
    /// - RfqItem contains: ItemId (cross-module), Quantity, UomId (cross-module)
    /// - RfqSupplier contains: SupplierId (cross-module)
    /// - RfqStatusId is auto-set to "SUBMIT" during creation
    ///
    /// Constructor requires: ApplicationDbContext, IIPAddressService, IUnitLookup
    ///
    /// CreateAsync requires MiscMaster chain (RfqStatus -> SUBMIT) to be seeded.
    /// IsDraftAsync, GetAggregateTrackingAsync are testable with seeded data.
    /// </summary>
    [Collection("DatabaseCollection")]
    public sealed class RfqCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public RfqCommandRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private RfqCommandRepository CreateRepo(ApplicationDbContext ctx)
        {
            var unitLookup = new Mock<IUnitLookup>(MockBehavior.Loose);
            var docSequence = new Mock<IDocumentSequenceLookup>(MockBehavior.Loose);
            return new RfqCommandRepository(ctx, _fixture.IpMock.Object, unitLookup.Object, docSequence.Object);
        }

        [Fact]
        public void Repository_Should_Be_Instantiable()
        {
            using var ctx = _fixture.CreateFreshDbContext();
            var repo = CreateRepo(ctx);
            repo.Should().NotBeNull();
        }

        [Fact]
        public async Task GetAggregateTrackingAsync_Should_Return_Null_When_NotFound()
        {
            await _fixture.ClearAllTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var result = await CreateRepo(ctx).GetAggregateTrackingAsync(9999, CancellationToken.None);

            result.Should().BeNull();
        }

        [Fact]
        public async Task IsDraftAsync_Should_Throw_When_DraftStatusNotSeeded()
        {
            await _fixture.ClearAllTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            // IsDraftAsync internally looks up "DRAFT" status code from MiscMaster.
            // Without seeded status data, it throws InvalidOperationException.
            Func<Task> act = async () => await CreateRepo(ctx).IsDraftAsync(9999, CancellationToken.None);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*DRAFT*not found*");
        }
    }
}
