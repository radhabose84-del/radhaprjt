using AutoMapper;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IGRN.IGRNEntry;
using PurchaseManagement.Application.GRN.GRNEntry.Queries.GetGateEntryPending;

namespace PurchaseManagement.UnitTests.Application.GRN.GRNEntry.Queries
{
    public sealed class GetGateEntryPendingQueryHandlerTests
    {
        private readonly Mock<IGRNEntryQueryRepository> _mockRepo = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetGateEntryPendingQueryHandler CreateSut() =>
            new(_mockRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_EmptyResult_ReturnsEmptyList()
        {
            _mockRepo
                .Setup(r => r.GetPendingPoGateAsync(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(new List<GetGateEntryPendingDto>());
            _mockMapper
                .Setup(m => m.Map<List<GetGateEntryPendingDto>>(It.IsAny<object>()))
                .Returns(new List<GetGateEntryPendingDto>());
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetGateEntryPendingQuery { PartyId = 1, PoId = 1 }, CancellationToken.None);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_CallsRepositoryOnce()
        {
            _mockRepo
                .Setup(r => r.GetPendingPoGateAsync(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(new List<GetGateEntryPendingDto>());
            _mockMapper
                .Setup(m => m.Map<List<GetGateEntryPendingDto>>(It.IsAny<object>()))
                .Returns(new List<GetGateEntryPendingDto>());
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(
                new GetGateEntryPendingQuery { PartyId = 1, PoId = 2 }, CancellationToken.None);

            _mockRepo.Verify(r => r.GetPendingPoGateAsync(1, 2), Times.Once);
        }
    }
}
