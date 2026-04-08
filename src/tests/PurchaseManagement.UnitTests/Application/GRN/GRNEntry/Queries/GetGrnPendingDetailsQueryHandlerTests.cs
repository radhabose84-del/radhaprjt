using AutoMapper;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Party;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IGRN.IGRNEntry;
using PurchaseManagement.Application.GRN.GRNEntry.Queries.GetGrnPendingDetails;

namespace PurchaseManagement.UnitTests.Application.GRN.GRNEntry.Queries
{
    public sealed class GetGrnPendingDetailsQueryHandlerTests
    {
        private readonly Mock<IGRNEntryQueryRepository> _mockRepo = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IItemPurchaseToleranceLookup> _mockToleranceLookup = new(MockBehavior.Loose);
        private readonly Mock<IPartyLookup> _mockPartyLookup = new(MockBehavior.Loose);

        private GetGrnPendingDetailsQueryHandler CreateSut() =>
            new(_mockRepo.Object, _mockMapper.Object, _mockMediator.Object,
                _mockToleranceLookup.Object, _mockPartyLookup.Object);

        [Fact]
        public void Constructor_CreatesHandler()
        {
            var sut = CreateSut();
            sut.Should().NotBeNull();
        }

        [Fact]
        public void QueryClass_Properties_ShouldBeAssignable()
        {
            var query = new GetGrnPendingDetailsQuery { GrnId = 1, IsGrnGenerated = true, IsQcGenerated = false };
            query.GrnId.Should().Be(1);
            query.IsGrnGenerated.Should().BeTrue();
            query.IsQcGenerated.Should().BeFalse();
        }
    }
}
