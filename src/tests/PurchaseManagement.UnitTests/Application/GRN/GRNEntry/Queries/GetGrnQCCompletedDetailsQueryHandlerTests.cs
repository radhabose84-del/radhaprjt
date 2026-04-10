using AutoMapper;
using Contracts.Interfaces.Lookups.Inventory;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IGRN.IGRNEntry;
using PurchaseManagement.Application.GRN.GRNEntry.Queries.GetGrnQCCompletedDetails;

namespace PurchaseManagement.UnitTests.Application.GRN.GRNEntry.Queries
{
    public sealed class GetGrnQCCompletedDetailsQueryHandlerTests
    {
        private readonly Mock<IGRNEntryQueryRepository> _mockRepo = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IItemPurchaseToleranceLookup> _mockToleranceLookup = new(MockBehavior.Loose);
        private readonly Mock<IPutawayRuleLookup> _mockPutawayRuleLookup = new(MockBehavior.Loose);

        private GetGrnQCCompletedDetailsQueryHandler CreateSut() =>
            new(_mockRepo.Object, _mockMapper.Object, _mockMediator.Object,
                _mockToleranceLookup.Object, _mockPutawayRuleLookup.Object);

        [Fact]
        public void Constructor_CreatesHandler()
        {
            var sut = CreateSut();
            sut.Should().NotBeNull();
        }

        [Fact]
        public void QueryClass_Properties_ShouldBeAssignable()
        {
            var query = new GetGrnQCCompletedDetailsQuery { GrnId = 5, ItemId = 10 };
            query.GrnId.Should().Be(5);
            query.ItemId.Should().Be(10);
        }
    }
}
