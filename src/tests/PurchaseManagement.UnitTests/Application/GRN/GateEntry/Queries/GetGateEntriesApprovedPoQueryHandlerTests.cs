using AutoMapper;
using Contracts.Dtos.Lookups.Party;
using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Party;
using Contracts.Interfaces.Lookups.Users;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IGRN.IGateEntry;
using PurchaseManagement.Application.GRN.GateEntry.Queries.GetGateEntriesApprovedPo;

namespace PurchaseManagement.UnitTests.Application.GRN.GateEntry.Queries
{
    public sealed class GetGateEntriesApprovedPoQueryHandlerTests
    {
        private readonly Mock<IGateEntryQueryRepository> _mockRepo = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IPartyLookup> _mockPartyLookup = new(MockBehavior.Loose);
        private readonly Mock<IUnitLookup> _mockUnitLookup = new(MockBehavior.Loose);

        private GetGateEntriesApprovedPoQueryHandler CreateSut() =>
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
                .Setup(r => r.GetGateEntriesApprovedPoDto(It.IsAny<int>()))
                .ReturnsAsync(new List<GetGateEntriesApprovedPoDto>());
            _mockMapper
                .Setup(m => m.Map<List<GetGateEntriesApprovedPoDto>>(It.IsAny<object>()))
                .Returns(new List<GetGateEntriesApprovedPoDto>());
            SetupLookupMocks();

            var result = await CreateSut().Handle(
                new GetGateEntriesApprovedPoQuery { PartyId = 1 }, CancellationToken.None);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_CallsRepositoryWithPartyId()
        {
            _mockRepo
                .Setup(r => r.GetGateEntriesApprovedPoDto(5))
                .ReturnsAsync(new List<GetGateEntriesApprovedPoDto>());
            _mockMapper
                .Setup(m => m.Map<List<GetGateEntriesApprovedPoDto>>(It.IsAny<object>()))
                .Returns(new List<GetGateEntriesApprovedPoDto>());
            SetupLookupMocks();

            await CreateSut().Handle(
                new GetGateEntriesApprovedPoQuery { PartyId = 5 }, CancellationToken.None);

            _mockRepo.Verify(r => r.GetGateEntriesApprovedPoDto(5), Times.Once);
        }
    }
}
