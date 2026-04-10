using AutoMapper;
using Contracts.Interfaces.Lookups.Inventory;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IGRN.IGRNEntry;
using PurchaseManagement.Application.GRN.GRNEntry.Queries.GetGrnPending;

namespace PurchaseManagement.UnitTests.Application.GRN.GRNEntry.Queries
{
    public sealed class GetGrnPendingQueryHandlerTests
    {
        private readonly Mock<IGRNEntryQueryRepository> _mockRepo = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IItemPurchaseToleranceLookup> _mockToleranceLookup = new(MockBehavior.Loose);

        private GetGrnPendingQueryHandler CreateSut() =>
            new(_mockRepo.Object, _mockMapper.Object, _mockMediator.Object, _mockToleranceLookup.Object);

        [Fact]
        public async Task Handle_EmptyResult_ReturnsEmptyList()
        {
            _mockRepo
                .Setup(r => r.GetPendingPoGrnAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(new List<GetGrnPendingDto>());
            _mockMapper
                .Setup(m => m.Map<List<GetGrnPendingDto>>(It.IsAny<object>()))
                .Returns(new List<GetGrnPendingDto>());

            var result = await CreateSut().Handle(
                new GetGrnPendingQuery { PartyId = 1, PoId = 1, GateEntryId = 1 },
                CancellationToken.None);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_CallsRepositoryOnce()
        {
            _mockRepo
                .Setup(r => r.GetPendingPoGrnAsync(1, 2, 3))
                .ReturnsAsync(new List<GetGrnPendingDto>());
            _mockMapper
                .Setup(m => m.Map<List<GetGrnPendingDto>>(It.IsAny<object>()))
                .Returns(new List<GetGrnPendingDto>());

            await CreateSut().Handle(
                new GetGrnPendingQuery { PartyId = 1, PoId = 2, GateEntryId = 3 },
                CancellationToken.None);

            _mockRepo.Verify(r => r.GetPendingPoGrnAsync(1, 2, 3), Times.Once);
        }
    }
}
