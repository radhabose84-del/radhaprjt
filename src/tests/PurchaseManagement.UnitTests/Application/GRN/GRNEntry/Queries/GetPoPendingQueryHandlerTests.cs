using AutoMapper;
using Contracts.Dtos.Lookups.Party;
using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Party;
using Contracts.Interfaces.Lookups.Users;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IGRN.IGRNEntry;
using PurchaseManagement.Application.GRN.GRNEntry.Queries.GetPoPending;

namespace PurchaseManagement.UnitTests.Application.GRN.GRNEntry.Queries
{
    public sealed class GetPoPendingQueryHandlerTests
    {
        private readonly Mock<IGRNEntryQueryRepository> _mockRepo = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IPartyLookup> _mockPartyLookup = new(MockBehavior.Loose);
        private readonly Mock<IUnitLookup> _mockUnitLookup = new(MockBehavior.Loose);

        private GetPoPendingQueryHandler CreateSut() =>
            new(_mockRepo.Object, _mockMapper.Object, _mockMediator.Object,
                _mockPartyLookup.Object, _mockUnitLookup.Object);

        private void SetupLookupMocks()
        {
            _mockPartyLookup
                .Setup(p => p.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<PartyLookupDto>());
            _mockUnitLookup
                .Setup(u => u.GetAllUnitAsync())
                .ReturnsAsync(new List<UnitLookupDto>());
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsEmptyList()
        {
            _mockRepo
                .Setup(r => r.GetPoPendingAsync())
                .ReturnsAsync(new List<GetPoPendingDto>());
            _mockMapper
                .Setup(m => m.Map<List<GetPoPendingDto>>(It.IsAny<object>()))
                .Returns(new List<GetPoPendingDto>());
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            SetupLookupMocks();

            var result = await CreateSut().Handle(new GetPoPendingQuery(), CancellationToken.None);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_CallsRepositoryOnce()
        {
            _mockRepo
                .Setup(r => r.GetPoPendingAsync())
                .ReturnsAsync(new List<GetPoPendingDto>());
            _mockMapper
                .Setup(m => m.Map<List<GetPoPendingDto>>(It.IsAny<object>()))
                .Returns(new List<GetPoPendingDto>());
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            SetupLookupMocks();

            await CreateSut().Handle(new GetPoPendingQuery(), CancellationToken.None);

            _mockRepo.Verify(r => r.GetPoPendingAsync(), Times.Once);
        }
    }
}
