using AutoMapper;
using MediatR;
using PartyManagement.Application.Common.Interfaces.IPartyMaster;
using PartyManagement.Application.PartyMaster.Queries.GetPartyActivityLog;

namespace PartyManagement.UnitTests.Application.PartyMaster.Queries
{
    public sealed class GetPartyActivityLogQueryHandlerTests
    {
        private readonly Mock<IPartyActivityLogCommandRepository> _mockRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetPartyActivityLogQueryHandler CreateSut() => new(_mockRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsLogs()
        {
            _mockRepo.Setup(r => r.GetActivityLogsByPartyIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<PartyManagement.Domain.Entities.PartyActivityLog>());

            var result = await CreateSut().Handle(new GetPartyActivityLogQuery { PartyId = 1 }, CancellationToken.None);
            result.Should().NotBeNull();
        }
    }
}
