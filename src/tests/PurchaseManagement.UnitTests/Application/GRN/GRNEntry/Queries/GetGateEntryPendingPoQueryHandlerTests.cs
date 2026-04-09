using AutoMapper;
using Contracts.Interfaces.Lookups.Party;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IGRN.IGRNEntry;
using PurchaseManagement.Application.GRN.GRNEntry.Queries.GetGateEntryPendingPo;

namespace PurchaseManagement.UnitTests.Application.GRN.GRNEntry.Queries
{
    public sealed class GetGateEntryPendingPoQueryHandlerTests
    {
        private readonly Mock<IGRNEntryQueryRepository> _mockRepo = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IPartyLookup> _mockPartyLookup = new(MockBehavior.Loose);

        private GetGateEntryPendingPoQueryHandler CreateSut() =>
            new(_mockRepo.Object, _mockMapper.Object, _mockMediator.Object, _mockPartyLookup.Object);

        [Fact]
        public async Task Handle_EmptyResult_ReturnsEmptyList()
        {
            _mockRepo
                .Setup(r => r.GetPendingPoAsync(It.IsAny<int>()))
                .ReturnsAsync(new List<GetGateEntryPendingPoDto>());
            _mockMapper
                .Setup(m => m.Map<List<GetGateEntryPendingPoDto>>(It.IsAny<object>()))
                .Returns(new List<GetGateEntryPendingPoDto>());

            var result = await CreateSut().Handle(
                new GetGateEntryPendingPoQuery { PartyId = 1 }, CancellationToken.None);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_CallsRepositoryWithCorrectPartyId()
        {
            _mockRepo
                .Setup(r => r.GetPendingPoAsync(5))
                .ReturnsAsync(new List<GetGateEntryPendingPoDto>());
            _mockMapper
                .Setup(m => m.Map<List<GetGateEntryPendingPoDto>>(It.IsAny<object>()))
                .Returns(new List<GetGateEntryPendingPoDto>());

            await CreateSut().Handle(
                new GetGateEntryPendingPoQuery { PartyId = 5 }, CancellationToken.None);

            _mockRepo.Verify(r => r.GetPendingPoAsync(5), Times.Once);
        }
    }
}
